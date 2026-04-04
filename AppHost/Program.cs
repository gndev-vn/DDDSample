EnsureNodeJsOnPath();

var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", secret: true);
var mongoUserName = builder.AddParameter("mongodb-username", "identity_user", publishValueAsDefault: true);
var mongoPassword = builder.AddParameter("mongodb-password", secret: true);

var sqlServer = builder.AddSqlServer("sqlserver", sqlPassword, port: 1433);
var catalogDatabase = sqlServer.AddDatabase("catalog-db", "CatalogAPI");
var orderingDatabase = sqlServer.AddDatabase("ordering-db", "OrderingAPI");
var paymentDatabase = sqlServer.AddDatabase("payment-db", "PaymentAPI");

var mongoDb = builder.AddMongoDB("mongodb", port: 27017, userName: mongoUserName, password: mongoPassword)
    .WithDataVolume();
var identityDatabase = mongoDb.AddDatabase("identity-db", "identitydb");

var redis = builder.AddRedis("redis", port: 6379);
var kafka = builder.AddKafka("kafka", port: 9092)
    .WithDataVolume()
    .WithKafkaUI();

var catalogApi = builder.AddProject<Projects.CatalogAPI>("catalog-api", options =>
    {
        options.ExcludeLaunchProfile = true;
        options.ExcludeKestrelEndpoints = true;
    })
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
    .WithEnvironment("ASPNETCORE_PREVENTHOSTINGSTARTUP", "true")
    .WithEnvironment("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES", string.Empty)
    .WithEnvironment("Hosting__Restful__Http", "5100")
    .WithEnvironment("Kafka__BootstrapServers", kafka.Resource.ConnectionStringExpression)
    .WithHttpEndpoint(targetPort: 5100, port: 5100, name: "http", isProxied: false)
    .WithReference(catalogDatabase, "Default")
    .WithReference(redis, "Redis")
    .WaitFor(catalogDatabase)
    .WaitFor(redis)
    .WaitFor(kafka);

var orderingApi = builder.AddProject<Projects.OrderingAPI>("ordering-api", options =>
    {
        options.ExcludeLaunchProfile = true;
        options.ExcludeKestrelEndpoints = true;
    })
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
    .WithEnvironment("ASPNETCORE_PREVENTHOSTINGSTARTUP", "true")
    .WithEnvironment("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES", string.Empty)
    .WithEnvironment("Hosting__Restful__Http", "5104")
    .WithEnvironment("Kafka__BootstrapServers", kafka.Resource.ConnectionStringExpression)
    .WithHttpEndpoint(targetPort: 5104, port: 5104, name: "http", isProxied: false)
    .WithReference(orderingDatabase, "Default")
    .WithReference(redis, "Redis")
    .WithReference(catalogApi)
    .WaitFor(orderingDatabase)
    .WaitFor(redis)
    .WaitFor(kafka)
    .WaitFor(catalogApi);

var paymentApi = builder.AddProject<Projects.PaymentAPI>("payment-api", options =>
    {
        options.ExcludeLaunchProfile = true;
        options.ExcludeKestrelEndpoints = true;
    })
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
    .WithEnvironment("ASPNETCORE_PREVENTHOSTINGSTARTUP", "true")
    .WithEnvironment("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES", string.Empty)
    .WithEnvironment("Kafka__BootstrapServers", kafka.Resource.ConnectionStringExpression)
    .WithHttpEndpoint(targetPort: 5012, port: 5012, name: "http", isProxied: false)
    .WithReference(paymentDatabase, "Default")
    .WithReference(redis, "Redis")
    .WithReference(orderingApi)
    .WaitFor(paymentDatabase)
    .WaitFor(redis)
    .WaitFor(kafka)
    .WaitFor(orderingApi);

var identityApi = builder.AddProject<Projects.IdentityAPI>("identity-api", options =>
    {
        options.ExcludeLaunchProfile = true;
        options.ExcludeKestrelEndpoints = true;
    })
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
    .WithEnvironment("ASPNETCORE_PREVENTHOSTINGSTARTUP", "true")
    .WithEnvironment("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES", string.Empty)
    .WithEnvironment("Hosting__Restful__Http", "5108")
    .WithHttpEndpoint(targetPort: 5108, port: 5108, name: "http", isProxied: false)
    .WithReference(identityDatabase, "MongoDB")
    .WithReference(redis, "Redis")
    .WaitFor(identityDatabase)
    .WaitFor(redis);

builder.AddNpmApp("web-app", @"..\WebApp", "dev")
    .WithHttpEndpoint(port: 5173, targetPort: 5173, name: "http", env: "PORT", isProxied: false)
    .WithExternalHttpEndpoints()
    .WithEnvironment("CATALOG_API_URL", catalogApi.GetEndpoint("http"))
    .WithEnvironment("ORDERING_API_URL", orderingApi.GetEndpoint("http"))
    .WithEnvironment("PAYMENT_API_URL", paymentApi.GetEndpoint("http"))
    .WithEnvironment("IDENTITY_API_URL", identityApi.GetEndpoint("http"))
    .WaitFor(catalogApi)
    .WaitFor(orderingApi)
    .WaitFor(paymentApi)
    .WaitFor(identityApi);

await builder.Build().RunAsync();

static void EnsureNodeJsOnPath()
{
    var existingPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
    var knownInstallDirectories = new[]
    {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "nodejs"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "nodejs")
    };

    var validInstallDirectory = knownInstallDirectories
        .FirstOrDefault(directory => Directory.Exists(directory) && File.Exists(Path.Combine(directory, "node.exe")));

    if (validInstallDirectory is null)
    {
        return;
    }

    var pathEntries = existingPath.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (pathEntries.Contains(validInstallDirectory, StringComparer.OrdinalIgnoreCase))
    {
        return;
    }

    Environment.SetEnvironmentVariable("PATH", $"{validInstallDirectory}{Path.PathSeparator}{existingPath}");
    Console.WriteLine($"Added Node.js to PATH for AppHost from '{validInstallDirectory}'.");
}
