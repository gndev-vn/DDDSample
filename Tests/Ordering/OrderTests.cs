using OrderingAPI.Domain.Entities;
using OrderingAPI.Domain.Events;
using Shared.Models;
using Shared.ValueObjects;

namespace DDDSample.Tests.Ordering;

public sealed class OrderTests
{
    [Fact]
    public void Create_PreservesCustomerSnapshotAndBillingAddress()
    {
        var customerId = Guid.NewGuid();
        var shipping = new Address("123 Main", null, "Ward 1", "District 1", "HCMC", "HCM");
        var billing = new Address("999 Billing", null, "Ward 2", "District 2", "HCMC", "HCM");
        var lines = new[]
        {
            new OrderLine(new Sku("SKU-1"), Quantity.Of(2), new Money(10m, "usd"))
        };

        var order = Order.Create(customerId, "Alex Nguyen", "alex@example.com", "+84 901 000 111", lines, shipping, billing);

        Assert.Equal(customerId, order.CustomerId);
        Assert.Equal("Alex Nguyen", order.CustomerName);
        Assert.Equal("alex@example.com", order.CustomerEmail);
        Assert.Equal("+84 901 000 111", order.CustomerPhone);
        Assert.Equal(billing, order.BillingAddress);
        Assert.Equal(shipping, order.ShippingAddress);
    }

    [Fact]
    public void Create_ComputesTotalFromLineQuantity()
    {
        var shipping = new Address("123 Main", null, "Ward 1", "District 1", "HCMC", "HCM");
        var lines = new[]
        {
            new OrderLine(new Sku("SKU-1"), Quantity.Of(2), new Money(10m, "USD")),
            new OrderLine(new Sku("SKU-2"), Quantity.Of(3), new Money(5m, "USD"))
        };

        var order = Order.Create(Guid.NewGuid(), "Alex Nguyen", "alex@example.com", null, lines, shipping);

        Assert.Equal(35m, order.Total.Amount);
        Assert.Equal("USD", order.Total.Currency);
    }

    [Fact]
    public void AddLines_RaisesOrderLineAddedEventWithOrderAndProductIdentifiers()
    {
        var shipping = new Address("123 Main", null, "Ward 1", "District 1", "HCMC", "HCM");
        var order = Order.Create(
            Guid.NewGuid(),
            "Alex Nguyen",
            "alex@example.com",
            null,
            [new OrderLine(new Sku("SKU-1"), Quantity.Of(1), new Money(10m, "USD"))],
            shipping);
        order.ClearDomainEvents();

        var productId = Guid.NewGuid();
        order.AddLines([
            new OrderLine(new Sku("SKU-2"), Quantity.Of(3), new Money(12m, "USD"))
            {
                ProductId = productId
            }
        ]);

        var domainEvent = Assert.IsType<OrderLineAddedDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, domainEvent.OrderId);
        Assert.Equal(productId, domainEvent.ProductId);
        Assert.Equal(3, domainEvent.Quantity);
        Assert.Equal(36m, domainEvent.Total);
        Assert.Equal("USD", domainEvent.Currency);
    }
}
