using CatalogAPI.Domain;
using CatalogAPI.Features;
using CatalogAPI.Configuration;
using CatalogAPI.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults;
using Shared.Authentication;
using Shared.Configuration;
using Shared.Extensions;
using Shared.Hosting;
using Shared.Interceptors;
using Shared.Middleware;
using Shared.Services;
using FluentValidation;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.RabbitMQ;
using Wolverine.SqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddCentralizedApiEndpoints();

// Service configuration
builder.Services.AddMapster();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.Configure<CatalogSeedOptions>(builder.Configuration.GetSection("CatalogSeed"));
builder.Services.AddScoped<CatalogSeedService>();

// Redis for token blacklist
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
});

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();

builder.Services.AddMediator(options =>
    {
        options.Assemblies = [typeof(Program)];
        options.ServiceLifetime = ServiceLifetime.Scoped;
    }
);

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
    var rabbitMqUrl = builder.Configuration.GetRabbitMqConnectionString();
    var exchange = builder.Configuration["Wolverine:CatalogExchange"] ?? throw new InvalidOperationException();

    opts.UseRabbitMq(rabbitMqUrl)
        .DeclareExchange(exchange, ex => { ex.ExchangeType = ExchangeType.Topic; })
        .AutoProvision();

    opts.LocalQueue(localEventsQueue).MaximumParallelMessages(8);
    opts.PublishAllMessages().ToRabbitTopics(exchange);

    opts.PersistMessagesWithSqlServer(sqlConnectionString, schema: "wolverine");
    opts.UseEntityFrameworkCoreTransactions();
    opts.Policies.UseDurableLocalQueues();
});

var app = builder.Build();

// Global exception handler
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Basic middleware
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<JwtBlacklistMiddleware>();
app.UseAuthorization();

// REST API endpoints
app.MapCatalogEndpoints();
app.MapOpenApi();
app.MapDefaultEndpoints();

// Run migration on startup
await app.Services.MigrateSqlServerDbContextAsync<AppDbContext>();
await using (var scope = app.Services.CreateAsyncScope())
{
    var catalogSeedService = scope.ServiceProvider.GetRequiredService<CatalogSeedService>();
    await catalogSeedService.SeedAsync();
}

// Start the application
await app.StartAsync();
await WolverineSqlServerDurabilityIndexingExtensions.EnsureWolverineSqlServerDurabilityIndexesAsync(sqlConnectionString, app.Logger);
await app.WaitForShutdownAsync();
