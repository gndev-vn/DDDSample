using Wolverine.Attributes;

namespace Shared.Messaging.Order;

[Topic("ordering.order.created")]
public class OrderCreatedEvent
{
    public Guid Id { get; set; }

    public string ShippingAddress { get; set; } = string.Empty;

    public string BillingAddress { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public string Currency { get; set; } = string.Empty;
}