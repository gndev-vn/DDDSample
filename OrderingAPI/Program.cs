using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using OrderingAPI.Configuration;
using OrderingAPI.Domain;
using OrderingAPI.Features;
using OrderingAPI.Services;
using ServiceDefaults;
using Shared.Authentication;
using Shared.Configuration;
using Shared.Extensions;
using Shared.Hosting;
using Shared.Interceptors;
using Shared.Middleware;
using Shared.Services;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.RabbitMQ;
using Wolverine.SqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddCentralizedApiEndpoints();

builder.Services.AddMediator(options =>
    {
        options.Assemblies = [typeof(Program)];
        options.ServiceLifetime = ServiceLifetime.Scoped;
    }
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.Configure<OrderingSeedOptions>(builder.Configuration.GetSection("OrderingSeed"));

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.AddServiceDiscovery();
builder.Services.AddStackExchangeRedisCache(o =>
{
    o.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "redis:6379";
});
builder.Services.AddHybridCache(o =>
{
    o.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromSeconds(30)
    };
});

var localEventsQueue = builder.Configuration["Wolverine:LocalQueue"] ?? throw new InvalidOperationException();
var sqlConnectionString = builder.Configuration.GetConnectionString("Default") ?? throw new InvalidOperationException();
var catalogApiAddress = builder.Configuration.GetSection("Services:catalog-api").Exists()
    ? "http://catalog-api"
    : builder.Configuration.GetValue<string>("RestServices:Catalog") ?? "http://localhost:5000";

builder.Services.AddDbContext<AppDbContext>((sp, opt) =>
{
    opt.UseSqlServer(sqlConnectionString, sql => sql.EnableRetryOnFailure());

    var domainEventInterceptor = new DomainEventInterceptor(
        sp.GetRequiredService<IMessageBus>(), localEventsQueue,
        sp.GetRequiredService<ILogger<DomainEventInterceptor>>());
    opt.AddInterceptors(domainEventInterceptor);
});

builder.Host.UseWolverine(opts =>
{
    var rabbitMqUrl = builder.Configuration.GetRabbitMqConnectionString();

    var catalogExchange = builder.Configuration["Wolverine:CatalogExchange"] ?? throw new InvalidOperationException();
    var orderingExchange = builder.Configuration["Wolverine:OrderingExchange"] ?? throw new InvalidOperationException();
    var paymentExchange = builder.Configuration["Wolverine:PaymentExchange"] ?? throw new InvalidOperationException();
    var orderingCatalogQueue = builder.Configuration["Wolverine:OrderingCatalogQueue"] ?? throw new InvalidOperationException();
    var orderingPaymentQueue = builder.Configuration["Wolverine:OrderingPaymentQueue"] ?? throw new InvalidOperationException();

    opts.UseRabbitMq(rabbitMqUrl)
        .DeclareExchange(orderingExchange, ex => ex.ExchangeType = ExchangeType.Topic)
        .DeclareExchange(catalogExchange, ex =>
        {
            ex.ExchangeType = ExchangeType.Topic;
            ex.BindTopic("catalog.category.created").ToQueue(orderingCatalogQueue);
            ex.BindTopic("catalog.category.updated").ToQueue(orderingCatalogQueue);
            ex.BindTopic("catalog.category.deleted").ToQueue(orderingCatalogQueue);
            ex.BindTopic("catalog.product.created").ToQueue(orderingCatalogQueue);
            ex.BindTopic("catalog.product.updated").ToQueue(orderingCatalogQueue);
            ex.BindTopic("catalog.product.deleted").ToQueue(orderingCatalogQueue);
        })
        .DeclareExchange(paymentExchange, ex =>
        {
            ex.ExchangeType = ExchangeType.Topic;
            ex.BindTopic("payment.completed").ToQueue(orderingPaymentQueue);
        })
        .DeclareQueue(orderingCatalogQueue)
        .DeclareQueue(orderingPaymentQueue)
        .AutoProvision();

    opts.LocalQueue(localEventsQueue).MaximumParallelMessages(8);
    opts.PublishAllMessages().ToRabbitTopics(orderingExchange);
    opts.ListenToRabbitQueue(orderingCatalogQueue);
    opts.ListenToRabbitQueue(orderingPaymentQueue);

    opts.PersistMessagesWithSqlServer(sqlConnectionString, schema: "wolverine");
    opts.UseEntityFrameworkCoreTransactions();
    opts.Policies.UseDurableLocalQueues();
});

builder.Services.AddHttpClient<ICatalogProductApiClient, CatalogProductApiClient>(client =>
{
    client.BaseAddress = new Uri(catalogApiAddress);
});
builder.Services.AddScoped<IProductLookupService, ProductLookupService>();
builder.Services.AddScoped<OrderingSeedService>();

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<JwtBlacklistMiddleware>();
app.UseAuthorization();

app.MapOrderingEndpoints();
app.MapOpenApi();
app.MapDefaultEndpoints();

await app.Services.MigrateSqlServerDbContextAsync<AppDbContext>();
await using (var scope = app.Services.CreateAsyncScope())
{
    var orderingSeedService = scope.ServiceProvider.GetRequiredService<OrderingSeedService>();
    await orderingSeedService.SeedAsync();
}

await app.StartAsync();
await WolverineSqlServerDurabilityIndexingExtensions.EnsureWolverineSqlServerDurabilityIndexesAsync(sqlConnectionString, app.Logger);
await app.WaitForShutdownAsync();
