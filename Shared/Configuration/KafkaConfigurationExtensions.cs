using Microsoft.Extensions.Configuration;

namespace Shared.Configuration;

public static class KafkaConfigurationExtensions
{
    public static string GetKafkaBootstrapServers(this IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("Kafka");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        var bootstrapServers = configuration["Kafka:BootstrapServers"]
            ?? configuration["KAFKA_BOOTSTRAP_SERVERS"];
        if (!string.IsNullOrWhiteSpace(bootstrapServers))
        {
            return bootstrapServers;
        }

        var host = configuration["Kafka:Host"] ?? configuration["KAFKA_HOST"];
        var port = configuration["Kafka:Port"] ?? configuration["KAFKA_PORT"];
        if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(port))
        {
            return $"{host}:{port}";
        }

        throw new InvalidOperationException("Kafka bootstrap servers are not configured.");
    }
}
