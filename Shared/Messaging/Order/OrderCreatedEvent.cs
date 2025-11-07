using Wolverine.Attributes;

namespace Shared.Messaging.Order;

[Topic("ordering.order.updated")]
public class OrderCreatedEvent
{
    public Guid Id { get; set; }

    public string ShippingAddress { get; set; }

    public string BillingAddress { get; set; }

    public decimal Total { get; set; }

    public string Currency { get; set; }
}