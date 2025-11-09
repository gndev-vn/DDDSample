using Mediator;
using OrderingAPI.Domain;

namespace OrderingAPI.Features.Orders.Commands;

public record DeleteOrderCommand(Guid Id) : IRequest<bool>;

public class DeleteOrderCommandHandler(AppDbContext dbContext) : IRequestHandler<DeleteOrderCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FindAsync([command.Id, cancellationToken],
            cancellationToken: cancellationToken);
        if (order == null)
        {
            return false;
        }

        dbContext.Orders.Remove(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}