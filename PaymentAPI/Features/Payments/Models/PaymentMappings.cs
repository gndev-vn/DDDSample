using System.Linq.Expressions;
using PaymentAPI.Domain.Entities;

namespace PaymentAPI.Features.Payments.Models;

public static class PaymentMappings
{
    public static readonly Expression<Func<Payment, PaymentModel>> Projection = payment => new PaymentModel
    {
        Id = payment.Id,
        OrderId = payment.OrderId,
        Amount = payment.Amount.Amount,
        Currency = payment.Amount.Currency,
        Status = payment.Status,
        TransactionReference = payment.TransactionReference,
        FailureReason = payment.FailureReason,
        CreatedAtUtc = payment.CreatedAtUtc,
        UpdatedAtUtc = payment.UpdatedAtUtc,
        CompletedAtUtc = payment.CompletedAtUtc
    };

    public static PaymentModel ToModel(Payment payment)
    {
        return new PaymentModel
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount.Amount,
            Currency = payment.Amount.Currency,
            Status = payment.Status,
            TransactionReference = payment.TransactionReference,
            FailureReason = payment.FailureReason,
            CreatedAtUtc = payment.CreatedAtUtc,
            UpdatedAtUtc = payment.UpdatedAtUtc,
            CompletedAtUtc = payment.CompletedAtUtc
        };
    }
}
