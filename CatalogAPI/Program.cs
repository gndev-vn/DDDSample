using CatalogAPI.Domain;
using CatalogAPI.Features;
using CatalogAPI.Configuration;
using CatalogAPI.Services;
using Confluent.Kafka;
using FluentValidation;
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
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Kafka;
using Wolverine.SqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddCentralizedApiEndpoints();

builder.Services.AddMapster();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.Configure<CatalogSeedOptions>(builder.Configuration.GetSection("CatalogSeed"));
builder.Services.AddScoped<CatalogSeedService>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
});

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
var kafkaBootstrapServers = builder.Configuration.GetKafkaBootstrapServers();
var catalogTopicPartitions = builder.Configuration.GetValue<int?>("Wolverine:CatalogTopicPartitions") ?? 6;
var kafkaReplicationFactor = (short)(builder.Configuration.GetValue<int?>("Wolverine:KafkaReplicationFactor") ?? 1);
var deadLetterTopic = builder.Configuration["Wolverine:DeadLetterTopic"] ?? "ddd.kafka.dead-letter";

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
    opts.MessagePartitioning.ByPropertyNamed("Id", "CategoryId", "ProductId", "OrderId", "PaymentId");
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
        spec.NumPartitions = catalogTopicPartitions;
        spec.ReplicationFactor = kafkaReplicationFactor;
    });

    opts.PersistMessagesWithSqlServer(sqlConnectionString, schema: "wolverine");
    opts.UseEntityFrameworkCoreTransactions();
    opts.Policies.UseDurableLocalQueues();
});

var app = builder.Build();

app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<JwtBlacklistMiddleware>();
app.UseAuthorization();

app.MapCatalogEndpoints();
app.MapOpenApi();
app.MapDefaultEndpoints();

await app.Services.MigrateSqlServerDbContextAsync<AppDbContext>();
await using (var scope = app.Services.CreateAsyncScope())
{
    var catalogSeedService = scope.ServiceProvider.GetRequiredService<CatalogSeedService>();
    await catalogSeedService.SeedAsync();
}

await app.StartAsync();
await WolverineSqlServerDurabilityIndexingExtensions.EnsureWolverineSqlServerDurabilityIndexesAsync(sqlConnectionString, app.Logger);
await app.WaitForShutdownAsync();
