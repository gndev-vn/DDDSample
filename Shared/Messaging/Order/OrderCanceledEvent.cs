using Wolverine.Attributes;
using Shared.Messaging;

namespace Shared.Messaging.Order;

[Topic(KafkaTopics.Ordering.OrderCanceled)]
public sealed class OrderCanceledEvent
{
    public Guid Id { get; set; }
}
