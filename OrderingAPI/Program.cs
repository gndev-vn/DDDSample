using Confluent.Kafka;
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
using Shared.Messaging;
using Shared.Middleware;
using Shared.Services;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Kafka;
using Wolverine.SqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddCentralizedApiEndpoints();

builder.Services.AddMediator(options =>
{
    options.Assemblies = [typeof(Program)];
    options.ServiceLifetime = ServiceLifetime.Scoped;
});
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
var kafkaBootstrapServers = builder.Configuration.GetKafkaBootstrapServers();
var deadLetterTopic = builder.Configuration["Wolverine:DeadLetterTopic"] ?? "ddd.kafka.dead-letter";
var orderingTopicPartitions = builder.Configuration.GetValue<int?>("Wolverine:OrderingTopicPartitions") ?? 6;
var kafkaReplicationFactor = (short)(builder.Configuration.GetValue<int?>("Wolverine:KafkaReplicationFactor") ?? 1);
var orderingCatalogGroup = builder.Configuration["Wolverine:OrderingCatalogGroup"] ?? throw new InvalidOperationException();
var orderingPaymentGroup = builder.Configuration["Wolverine:OrderingPaymentGroup"] ?? throw new InvalidOperationException();
var catalogApiAddress = builder.Configuration.GetSection("Services:catalog-api").Exists()
    ? "http://catalog-api"
    : builder.Configuration.GetValue<string>("RestServices:Catalog") ?? "http://localhost:5000";

builder.Services.AddDbContext<AppDbContext>((sp, opt) =>
{
    opt.UseSqlServer(sqlConnectionString, sql => sql.EnableRetryOnFailure());

    var domainEventInterceptor = new DomainEventInterceptor(
        sp.GetRequiredService<IMessageBus>(),
        localEventsQueue,
        sp.GetRequiredService<ILogger<DomainEventInterceptor>>());
    opt.AddInterceptors(domainEventInterceptor);
});

builder.Host.UseWolverine(opts =>
{
    opts.MessagePartitioning.ByPropertyNamed("OrderId", "Id", "CategoryId", "ProductId", "PaymentId", "CustomerId");
    opts.Policies.PropagateGroupIdToPartitionKey();

    opts.UseKafka(kafkaBootstrapServers)
        .ConfigureProducers(config =>
        {
            config.Acks = Acks.All;
            config.EnableIdempotence = true;
            config.CompressionType = CompressionType.Snappy;
            config.MessageSendMaxRetries = 5;
            config.RetryBackoffMs = 250;
            config.AllowAutoCreateTopics = false;
        })
        .ConfigureConsumers(config =>
        {
            config.AllowAutoCreateTopics = false;
            config.AutoOffsetReset = AutoOffsetReset.Earliest;
        })
        .DeadLetterQueueTopicName(deadLetterTopic)
        .AutoProvision(_ => { });

    opts.LocalQueue(localEventsQueue).MaximumParallelMessages(8);
    opts.PublishAllMessages().ToKafkaTopics().Specification(spec =>
    {
        spec.NumPartitions = orderingTopicPartitions;
        spec.ReplicationFactor = kafkaReplicationFactor;
    });

    opts.ListenToKafkaTopics(KafkaTopics.Catalog.OrderingProjectionTopics)
        .ConfigureConsumer(config =>
        {
            config.GroupId = orderingCatalogGroup;
            config.AutoOffsetReset = AutoOffsetReset.Earliest;
        })
        .EnableNativeDeadLetterQueue();

    opts.ListenToKafkaTopics(KafkaTopics.Payment.OrderingWorkflowTopics)
        .ConfigureConsumer(config =>
        {
            config.GroupId = orderingPaymentGroup;
            config.AutoOffsetReset = AutoOffsetReset.Earliest;
        })
        .EnableNativeDeadLetterQueue();

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
