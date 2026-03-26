using OrderingAPI.Domain.Entities;
using Shared.Models;
using Shared.ValueObjects;

namespace DDDSample.Tests.Ordering;

public sealed class OrderTests
{
    [Fact]
    public void Create_PreservesCustomerAndBillingAddress()
    {
        var customerId = Guid.NewGuid();
        var shipping = new Address("123 Main", null, "Ward 1", "District 1", "HCMC", "HCM");
        var billing = new Address("999 Billing", null, "Ward 2", "District 2", "HCMC", "HCM");
        var lines = new[]
        {
            new OrderLine(new Sku("SKU-1"), Quantity.Of(2), new Money(10m, "usd"))
        };

        var order = Order.Create(customerId, lines, shipping, billing);

        Assert.Equal(customerId, order.CustomerId);
        Assert.Equal(billing, order.BillingAddress);
        Assert.Equal(shipping, order.ShippingAddress);
    }
}
