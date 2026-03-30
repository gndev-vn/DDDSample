using Mediator;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.Domain;
using PaymentAPI.Features.Payments.Models;

namespace PaymentAPI.Features.Payments.Queries;

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
