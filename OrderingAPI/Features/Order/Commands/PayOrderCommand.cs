using Mediator;
using OrderingAPI.Domain;

namespace OrderingAPI.Features.Order.Commands;

public record PayOrderCommand(Guid Id) : IRequest<bool>;

public class PayOrderCommandHandler(AppDbContext dbContext) : IRequestHandler<PayOrderCommand, bool>
{
    public async ValueTask<bool> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}