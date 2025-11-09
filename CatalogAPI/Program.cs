using CatalogAPI.Domain;
using CatalogAPI.GraphQL;
using CatalogAPI.Services;
using Mapster;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Shared.Authentication;
using Shared.Interceptors;
using Shared.Middleware;
using Shared.Services;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.RabbitMQ;
using Wolverine.SqlServer;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    var restfulHttpPort = builder.Configuration["Hosting:Restful:Http"] ?? throw new InvalidOperationException();
    // var restfulHttpsPort = builder.Configuration["Hosting:Restful:Https"] ?? throw new InvalidOperationException();
    var grpcHttpPort = builder.Configuration["Hosting:Grpc:Http"] ?? throw new InvalidOperationException();
    // var grpcHttpsPort = builder.Configuration["Hosting:Grpc:Https"] ?? throw new InvalidOperationException();

    // Setup a HTTP/1.1 endpoint for REST API
    options.ListenAnyIP(int.Parse(restfulHttpPort), o => o.Protocols = HttpProtocols.Http1);
    /*options.ListenAnyIP(int.Parse(restfulHttpsPort), o =>
    {
        o.Protocols = HttpProtocols.Http1;
        o.UseHttps();
    });*/
    // Setup a HTTP/2 endpoint for gRPC
    options.ListenAnyIP(int.Parse(grpcHttpPort), o => o.Protocols = HttpProtocols.Http2);
    /*options.ListenLocalhost(int.Parse(grpcHttpsPort), o =>
    {
        o.Protocols = HttpProtocols.Http2;
        o.UseHttps();
    });*/
});

// Service configuration
builder.Services.AddGrpc();
builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();
builder.Services.AddMapster();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

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
    opt.UseSqlServer(sqlConnectionString);
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
app.MapControllers();
app.MapOpenApi();
app.MapGraphQL();

// gRPC endpoints
app.MapGrpcService<ProductGrpcService>();
app.MapGrpcService<CategoryGrpcService>();

// Run migration on startup
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await db.Database.MigrateAsync();

// Start the application
await app.RunAsync();