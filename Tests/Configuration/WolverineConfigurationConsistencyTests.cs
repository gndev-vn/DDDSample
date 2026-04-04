using System.Text.Json;

namespace DDDSample.Tests.Configuration;

public sealed class WolverineConfigurationConsistencyTests
{
    [Fact]
    public void KafkaSettings_AreAlignedAcrossMessagingServices()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));

        using var catalog = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "CatalogAPI", "appsettings.json")));
        using var ordering = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "OrderingAPI", "appsettings.json")));
        using var payment = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "PaymentAPI", "appsettings.json")));

        var catalogKafka = catalog.RootElement.GetProperty("Kafka");
        var orderingKafka = ordering.RootElement.GetProperty("Kafka");
        var paymentKafka = payment.RootElement.GetProperty("Kafka");

        Assert.Equal(catalogKafka.GetProperty("BootstrapServers").GetString(), orderingKafka.GetProperty("BootstrapServers").GetString());
        Assert.Equal(orderingKafka.GetProperty("BootstrapServers").GetString(), paymentKafka.GetProperty("BootstrapServers").GetString());

        var catalogWolverine = catalog.RootElement.GetProperty("Wolverine");
        var orderingWolverine = ordering.RootElement.GetProperty("Wolverine");
        var paymentWolverine = payment.RootElement.GetProperty("Wolverine");

        Assert.Equal(catalogWolverine.GetProperty("DeadLetterTopic").GetString(), orderingWolverine.GetProperty("DeadLetterTopic").GetString());
        Assert.Equal(orderingWolverine.GetProperty("DeadLetterTopic").GetString(), paymentWolverine.GetProperty("DeadLetterTopic").GetString());

        Assert.Equal(catalogWolverine.GetProperty("KafkaReplicationFactor").GetInt32(), orderingWolverine.GetProperty("KafkaReplicationFactor").GetInt32());
        Assert.Equal(orderingWolverine.GetProperty("KafkaReplicationFactor").GetInt32(), paymentWolverine.GetProperty("KafkaReplicationFactor").GetInt32());
    }
}
