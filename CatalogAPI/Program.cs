using CatalogAPI.Domain;
using CatalogAPI.GraphQL;
using CatalogAPI.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Shared.Authentication;
using Shared.Hosting;
using Shared.Extensions;
using Shared.Interceptors;
using Shared.Middleware;
using Shared.Services;
using Shared.Validation;
using FluentValidation;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.RabbitMQ;
using Wolverine.SqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.AddCentralizedApiEndpoints();

// Service configuration
builder.Services.AddGrpc();
builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();
builder.Services.AddMapster();
builder.Services.AddScoped<RequestValidationActionFilter>();
builder.Services.AddControllers(options => options.Filters.AddService<RequestValidationActionFilter>());
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

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
await app.Services.MigrateSqlServerDbContextAsync<AppDbContext>();

// Start the application
await app.RunAsync();
