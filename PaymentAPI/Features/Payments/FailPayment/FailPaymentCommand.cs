using Mediator;
using PaymentAPI.Domain;
using PaymentAPI.Features.Payments.GetPaymentById;
using PaymentAPI.Features.Payments.UpdatePayment;

namespace PaymentAPI.Features.Payments.FailPayment;

public record FailPaymentCommand(Guid PaymentId, string Reason) : IRequest<PaymentModel>;

public class FailPaymentCommandHandler(AppDbContext dbContext) : IRequestHandler<FailPaymentCommand, PaymentModel>
{
    public async ValueTask<PaymentModel> Handle(FailPaymentCommand command, CancellationToken cancellationToken)
    {
        var payment = await dbContext.GetPaymentForUpdateAsync(command.PaymentId, cancellationToken);

        payment.Fail(command.Reason);
        await dbContext.SaveChangesAsync(cancellationToken);

        return PaymentMappings.ToModel(payment);
    }
}