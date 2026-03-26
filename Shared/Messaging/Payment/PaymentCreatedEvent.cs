using Wolverine.Attributes;

namespace Shared.Messaging.Payment;

[Topic("payment.created")]
public class PaymentCreatedEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
