using Mediator;
using OrderingAPI.Domain;

namespace OrderingAPI.Features.Order.Commands;

public record CancelOrderCommand(Guid Id) : IRequest<bool>;

public class CancelOrderCommandHandler(AppDbContext dbContext) : IRequestHandler<CancelOrderCommand, bool>
{
    public async ValueTask<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}