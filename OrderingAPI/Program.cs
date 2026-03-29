using System.Net;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using OrderingAPI.Domain;
using OrderingAPI.Services;
using OrderingAPI.Services.Grpc;
using Shared.Authentication;
using Shared.Hosting;
using Shared.Extensions;
using Shared.Interceptors;
using Shared.Middleware;
using Shared.Validation;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.RabbitMQ;
using Wolverine.SqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.AddCentralizedApiEndpoints();

builder.Services.AddMediator(options =>
    {
        options.Assemblies = [typeof(Program)];
        options.ServiceLifetime = ServiceLifetime.Scoped;
    }
);

builder.Services.AddGrpc();
builder.Services.AddScoped<RequestValidationActionFilter>();
builder.Services.AddControllers(options => options.Filters.AddService<RequestValidationActionFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<Shared.Services.ITokenBlacklistService, Shared.Services.TokenBlacklistService>();
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
    var rabbitMqHost = builder.Configuration["RabbitMq:Host"] ?? throw new InvalidOperationException();
    var rabbitMqUsername = builder.Configuration["RabbitMq:Username"] ?? throw new InvalidOperationException();
    var rabbitMqPassword = builder.Configuration["RabbitMq:Password"] ?? throw new InvalidOperationException();
    var rabbitMqUrl = string.Format("amqp://{1}:{2}@{0}", rabbitMqHost, rabbitMqUsername, rabbitMqPassword);

    var catalogExchange = builder.Configuration["Wolverine:CatalogExchange"] ?? throw new InvalidOperationException();
    var orderingExchange = builder.Configuration["Wolverine:OrderingExchange"] ?? throw new InvalidOperationException();
    var paymentExchange = builder.Configuration["Wolverine:PaymentExchange"] ?? throw new InvalidOperationException();
    var orderingCatalogQueue = builder.Configuration["Wolverine:OrderingCatalogQueue"] ??
                               throw new InvalidOperationException();
    var orderingPaymentQueue = builder.Configuration["Wolverine:OrderingPaymentQueue"] ??
                               throw new InvalidOperationException();

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

builder.Services.AddGrpcClient<ProductSvc.ProductSvcClient>(o =>
    {
        o.Address = new Uri(builder.Configuration.GetValue<string>("GrpcServices:Catalog") ??
                            "http://catalog-service:8081"); // h2c by default
    })
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        KeepAlivePingDelay = TimeSpan.FromSeconds(30),
        KeepAlivePingTimeout = TimeSpan.FromSeconds(15),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
        EnableMultipleHttp2Connections = true,
        AutomaticDecompression = DecompressionMethods.All,
    });
builder.Services.AddScoped<IProductGrpcClientService, ProductGrpcClientService>();
builder.Services.AddScoped<IProductLookupService, ProductLookupService>();

var app = builder.Build();

// Global exception handler
app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<JwtBlacklistMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapOpenApi();
app.MapGrpcService<OrderGrpcService>();

await app.Services.MigrateSqlServerDbContextAsync<AppDbContext>();

app.Run();
