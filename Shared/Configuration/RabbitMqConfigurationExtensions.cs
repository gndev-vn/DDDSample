using Microsoft.Extensions.Configuration;

namespace Shared.Configuration;

public static class RabbitMqConfigurationExtensions
{
    public static string GetRabbitMqConnectionString(this IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("RabbitMq");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        var host = configuration["RabbitMq:Host"] ?? throw new InvalidOperationException();
        var username = configuration["RabbitMq:Username"] ?? throw new InvalidOperationException();
        var password = configuration["RabbitMq:Password"] ?? throw new InvalidOperationException();

        return $"amqp://{username}:{password}@{host}";
    }
}
