using Mediator;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.Domain;
using PaymentAPI.Features.Payments.Models;

namespace PaymentAPI.Features.Payments.Queries;

public record GetPaymentByOrderIdQuery(Guid OrderId) : IRequest<PaymentModel?>;

public class GetPaymentByOrderIdQueryHandler(AppDbContext dbContext) : IRequestHandler<GetPaymentByOrderIdQuery, PaymentModel?>
{
    public async ValueTask<PaymentModel?> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Payments
            .AsNoTracking()
            .Where(x => x.OrderId == request.OrderId)
            .Select(PaymentMappings.Projection)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
