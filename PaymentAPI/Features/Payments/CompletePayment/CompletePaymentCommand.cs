using Mediator;
using PaymentAPI.Domain;
using PaymentAPI.Features.Payments.GetPaymentById;
using PaymentAPI.Features.Payments.UpdatePayment;

namespace PaymentAPI.Features.Payments.CompletePayment;

public record CompletePaymentCommand(Guid PaymentId, string TransactionReference) : IRequest<PaymentModel>;

public class CompletePaymentCommandHandler(AppDbContext dbContext) : IRequestHandler<CompletePaymentCommand, PaymentModel>
{
    public async ValueTask<PaymentModel> Handle(CompletePaymentCommand command, CancellationToken cancellationToken)
    {
        var payment = await dbContext.GetPaymentForUpdateAsync(command.PaymentId, cancellationToken);

        payment.Complete(command.TransactionReference);
        await dbContext.SaveChangesAsync(cancellationToken);

        return PaymentMappings.ToModel(payment);
    }
}