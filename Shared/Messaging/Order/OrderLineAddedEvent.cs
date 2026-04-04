using Wolverine.Attributes;
using Shared.Messaging;

namespace Shared.Messaging.Order;

[Topic(KafkaTopics.Ordering.OrderLineAdded)]
public sealed class OrderLineAddedEvent
{
    public Guid OrderId { get; set; }
    public Guid OrderLineId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = string.Empty;
}
