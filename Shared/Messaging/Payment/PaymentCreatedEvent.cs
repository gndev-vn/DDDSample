using Wolverine.Attributes;
using Shared.Messaging;

namespace Shared.Messaging.Payment;

[Topic(KafkaTopics.Payment.PaymentCreated)]
public sealed class PaymentCreatedEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
