using OrderingAPI.Domain.Entities;
using Shared.ValueObjects;

namespace DDDSample.Tests.Ordering;

public sealed class UpdateOrderCommandTests
{
    [Fact]
    public void UpdateOrder_ReplacesLinesAndShippingAddress()
    {
        var existingOrder = Order.Create(
            Guid.NewGuid(),
            [new OrderLine(new Sku("SKU-1"), Quantity.Of(1), new Money(10m, "USD"))],
            new Address("Old line", null, "Old ward", "Old district", "Old city", "Old province"));
        var updatedShipping = new Address("New line", null, "New ward", "New district", "New city", "New province");

        existingOrder.Update(
            updatedShipping,
            [new OrderLine(new Sku("SKU-2"), Quantity.Of(3), new Money(25m, "USD"))]);

        Assert.Equal("New line", existingOrder.ShippingAddress?.Line1);
        Assert.Single(existingOrder.Lines);
        Assert.Equal("SKU-2", existingOrder.Lines.Single().Sku.Value);
        Assert.Equal(3, existingOrder.Lines.Single().Quantity.Value);
    }
}
