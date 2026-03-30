using PaymentAPI.Domain.Enums;
using PaymentAPI.Domain.Events;
using Shared.Exceptions;
using Shared.Models;
using Shared.ValueObjects;

namespace PaymentAPI.Domain.Entities;

public class Payment : EntityWithEvents
{
    public Guid OrderId { get; private set; }
    public Money Amount { get; private set; } = new(0m, "USD");
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? TransactionReference { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    public Payment()
    {
    }

    private Payment(Guid orderId, Money amount)
    {
        if (orderId == Guid.Empty)
        {
            throw new DomainException("Order id is required");
        }

        Amount = amount ?? throw new ArgumentNullException(nameof(amount));
        OrderId = orderId;
        Status = PaymentStatus.Pending;
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new PaymentCreatedDomainEvent
        {
            PaymentId = Id,
            OrderId = orderId,
            Amount = amount.Amount,
            Currency = amount.Currency
        });
    }

    public static Payment CreatePending(Guid orderId, Money amount) => new(orderId, amount);

    public void Complete(string transactionReference)
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new DomainException("Only pending payments can be completed");
        }

        if (string.IsNullOrWhiteSpace(transactionReference))
        {
            throw new DomainException("Transaction reference is required");
        }

        TransactionReference = transactionReference.Trim();
        FailureReason = null;
        Status = PaymentStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new PaymentCompletedDomainEvent
        {
            PaymentId = Id,
            OrderId = OrderId,
            Amount = Amount.Amount,
            Currency = Amount.Currency,
            TransactionReference = TransactionReference
        });
    }

    public void Fail(string reason)
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new DomainException("Only pending payments can be marked as failed");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Failure reason is required");
        }

        FailureReason = reason.Trim();
        TransactionReference = null;
        Status = PaymentStatus.Failed;
        CompletedAtUtc = null;
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new PaymentFailedDomainEvent
        {
            PaymentId = Id,
            OrderId = OrderId,
            Amount = Amount.Amount,
            Currency = Amount.Currency,
            Reason = FailureReason
        });
    }
}
