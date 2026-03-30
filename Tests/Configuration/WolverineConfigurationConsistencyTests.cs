using System.Text.Json;

namespace DDDSample.Tests.Configuration;

public sealed class WolverineConfigurationConsistencyTests
{
    [Fact]
    public void PaymentAndOrdering_ProductionExchanges_AreAligned()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));

        using var ordering = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "OrderingAPI", "appsettings.json")));
        using var payment = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "PaymentAPI", "appsettings.json")));

        var orderingWolverine = ordering.RootElement.GetProperty("Wolverine");
        var paymentWolverine = payment.RootElement.GetProperty("Wolverine");

        Assert.Equal(
            orderingWolverine.GetProperty("OrderingExchange").GetString(),
            paymentWolverine.GetProperty("OrderingExchange").GetString());

        Assert.Equal(
            orderingWolverine.GetProperty("PaymentExchange").GetString(),
            paymentWolverine.GetProperty("PaymentExchange").GetString());
    }
}
