using Wolverine.Attributes;
using Shared.Messaging;

namespace Shared.Messaging.Order;

[Topic(KafkaTopics.Ordering.OrderUpdated)]
public sealed class OrderUpdatedEvent
{
    public Guid Id { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Currency { get; set; } = string.Empty;
}
