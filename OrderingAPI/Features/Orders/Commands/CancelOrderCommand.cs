using Mediator;
using OrderingAPI.Domain;
using Shared.Exceptions;

namespace OrderingAPI.Features.Orders.Commands;

public record CancelOrderCommand(Guid Id) : IRequest<bool>;

public class CancelOrderCommandHandler(AppDbContext dbContext) : IRequestHandler<CancelOrderCommand, bool>
{
    public async ValueTask<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FindAsync([request.Id, cancellationToken],
            cancellationToken: cancellationToken);
        if (order == null)
        {
            throw new NotFoundException("Order not found");
        }

        order.Cancel();
        dbContext.Update(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}