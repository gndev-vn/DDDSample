using Mediator;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.Domain;

namespace PaymentAPI.Features.Payments.GetPaymentById;

public record GetPaymentByIdQuery(Guid Id) : IRequest<PaymentModel?>;

public class GetPaymentByIdQueryHandler(AppDbContext dbContext) : IRequestHandler<GetPaymentByIdQuery, PaymentModel?>
{
    public async ValueTask<PaymentModel?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Payments
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(PaymentMappings.Projection)
            .FirstOrDefaultAsync(cancellationToken);
    }
}