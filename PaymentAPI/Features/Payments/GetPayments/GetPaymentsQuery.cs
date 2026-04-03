using Mediator;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.Domain;
using PaymentAPI.Features.Payments.GetPaymentById;

namespace PaymentAPI.Features.Payments.GetPayments;

public record GetPaymentsQuery : IRequest<List<PaymentModel>>;

public class GetPaymentsQueryHandler(AppDbContext dbContext) : IRequestHandler<GetPaymentsQuery, List<PaymentModel>>
{
    public async ValueTask<List<PaymentModel>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Payments
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(PaymentMappings.Projection)
            .ToListAsync(cancellationToken);
    }
}