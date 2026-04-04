using Confluent.Kafka;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.Domain;
using PaymentAPI.Features;
using PaymentAPI.Features.Payments.CreatePayment;
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

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.AddServiceDiscovery();
builder.Services.AddStackExchangeRedisCache(o =>
{
    o.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "redis:6379";
});

var localEventsQueue = builder.Configuration["Wolverine:LocalQueue"] ?? throw new InvalidOperationException();
var sqlConnectionString = builder.Configuration.GetConnectionString("Default") ?? throw new InvalidOperationException();
var kafkaBootstrapServers = builder.Configuration.GetKafkaBootstrapServers();
var paymentOrderingGroup = builder.Configuration["Wolverine:PaymentOrderingGroup"] ?? throw new InvalidOperationException();
var paymentTopicPartitions = builder.Configuration.GetValue<int?>("Wolverine:PaymentTopicPartitions") ?? 6;
var orderingTopicPartitions = builder.Configuration.GetValue<int?>("Wolverine:OrderingTopicPartitions") ?? 6;
var kafkaReplicationFactor = (short)(builder.Configuration.GetValue<int?>("Wolverine:KafkaReplicationFactor") ?? 1);
var deadLetterTopic = builder.Configuration["Wolverine:DeadLetterTopic"] ?? "ddd.kafka.dead-letter";

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
    opts.MessagePartitioning.ByPropertyNamed("OrderId", "Id", "PaymentId", "CustomerId");
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
    foreach (var topic in KafkaTopics.Ordering.PaymentWorkflowTopics)
    {
        opts.ListenToKafkaTopic(topic)
            .Specification(spec =>
            {
                spec.NumPartitions = orderingTopicPartitions;
                spec.ReplicationFactor = kafkaReplicationFactor;
            })
            .ConfigureConsumer(config =>
            {
                config.GroupId = paymentOrderingGroup;
                config.AutoOffsetReset = AutoOffsetReset.Earliest;
            })
            .EnableNativeDeadLetterQueue();
    }

    opts.PublishAllMessages().ToKafkaTopics().Specification(spec =>
    {
        spec.NumPartitions = paymentTopicPartitions;
        spec.ReplicationFactor = kafkaReplicationFactor;
    });

    opts.PersistMessagesWithSqlServer(sqlConnectionString, schema: "wolverine");
    opts.UseEntityFrameworkCoreTransactions();
    opts.Policies.UseDurableLocalQueues();
});

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<JwtBlacklistMiddleware>();
app.UseAuthorization();

app.MapPaymentEndpoints();
app.MapOpenApi();
app.MapDefaultEndpoints();

await app.Services.MigrateSqlServerDbContextAsync<AppDbContext>();

await app.StartAsync();
await WolverineSqlServerDurabilityIndexingExtensions.EnsureWolverineSqlServerDurabilityIndexesAsync(sqlConnectionString, app.Logger);
await app.WaitForShutdownAsync();


