using Mediator;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.Domain;
using PaymentAPI.Features.Payments.Models;

namespace PaymentAPI.Features.Payments.Queries;

public record GetPaymentsQuery : IRequest<IEnumerable<PaymentModel>>;

public class GetPaymentsQueryHandler(AppDbContext dbContext) : IRequestHandler<GetPaymentsQuery, IEnumerable<PaymentModel>>
{
    public async ValueTask<IEnumerable<PaymentModel>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Payments
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(PaymentMappings.Projection)
            .ToListAsync(cancellationToken);
    }
}
