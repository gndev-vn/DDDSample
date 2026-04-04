using Wolverine.Attributes;
using Shared.Messaging;

namespace Shared.Messaging.Order;

[Topic(KafkaTopics.Ordering.OrderPaid)]
public sealed class OrderPaidEvent
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = string.Empty;
}
