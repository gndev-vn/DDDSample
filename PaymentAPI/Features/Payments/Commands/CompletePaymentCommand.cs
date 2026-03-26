using Mediator;
using PaymentAPI.Domain;
using PaymentAPI.Features.Payments.Models;
using Shared.Exceptions;

namespace PaymentAPI.Features.Payments.Commands;

public record CompletePaymentCommand(Guid PaymentId, string TransactionReference) : IRequest<PaymentModel>;

public class CompletePaymentCommandHandler(AppDbContext dbContext) : IRequestHandler<CompletePaymentCommand, PaymentModel>
{
    public async ValueTask<PaymentModel> Handle(CompletePaymentCommand command, CancellationToken cancellationToken)
    {
        var payment = await dbContext.Payments.FindAsync([command.PaymentId], cancellationToken: cancellationToken);
        if (payment == null)
        {
            throw new NotFoundException("Payment", command.PaymentId);
        }

        payment.Complete(command.TransactionReference);
        await dbContext.SaveChangesAsync(cancellationToken);
        return PaymentMappings.ToModel(payment);
    }
}
