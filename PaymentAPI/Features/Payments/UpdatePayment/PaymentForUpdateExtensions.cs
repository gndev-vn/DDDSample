using PaymentAPI.Domain;
using PaymentAPI.Domain.Entities;
using Shared.Exceptions;

namespace PaymentAPI.Features.Payments.UpdatePayment;

internal static class PaymentForUpdateExtensions
{
    public static async Task<Payment> GetPaymentForUpdateAsync(this AppDbContext dbContext, Guid paymentId, CancellationToken cancellationToken)
    {
        var payment = await dbContext.Payments.FindAsync([paymentId], cancellationToken: cancellationToken);
        if (payment is null)
        {
            throw new NotFoundException("Payment", paymentId);
        }

        return payment;
    }
}