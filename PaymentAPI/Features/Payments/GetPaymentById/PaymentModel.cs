using PaymentAPI.Domain.Enums;
using Shared.Models;

namespace PaymentAPI.Features.Payments.GetPaymentById;

public class PaymentModel : ModelBase
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string? TransactionReference { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}