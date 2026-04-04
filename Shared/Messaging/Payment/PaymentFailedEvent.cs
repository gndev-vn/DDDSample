using Wolverine.Attributes;
using Shared.Messaging;

namespace Shared.Messaging.Payment;

[Topic(KafkaTopics.Payment.PaymentFailed)]
public sealed class PaymentFailedEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
