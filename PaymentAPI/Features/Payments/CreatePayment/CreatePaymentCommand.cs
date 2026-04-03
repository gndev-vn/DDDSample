using Mediator;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.Domain;
using PaymentAPI.Domain.Entities;
using PaymentAPI.Features.Payments.GetPaymentById;
using Shared.ValueObjects;

namespace PaymentAPI.Features.Payments.CreatePayment;

public record CreatePaymentCommand(Guid OrderId, decimal Amount, string Currency) : IRequest<PaymentModel>;

public class CreatePaymentCommandHandler(AppDbContext dbContext) : IRequestHandler<CreatePaymentCommand, PaymentModel>
{
    public async ValueTask<PaymentModel> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        var existingPayment = await dbContext.Payments
            .AsNoTracking()
            .Where(payment => payment.OrderId == command.OrderId)
            .Select(PaymentMappings.Projection)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingPayment is not null)
        {
            return existingPayment;
        }

        var payment = Payment.CreatePending(command.OrderId, new Money(command.Amount, command.Currency));

        await dbContext.Payments.AddAsync(payment, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return PaymentMappings.ToModel(payment);
    }
}