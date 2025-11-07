using Mapster;
using Mediator;
using OrderingAPI.Domain;
using OrderingAPI.Features.Order.Models;

namespace OrderingAPI.Features.Order.Commands;

public record UpdateOrderCommand(OrderModel Model) : IRequest<OrderModel>;

public class UpdateOrderCommandHandler(AppDbContext dbContext) : IRequestHandler<UpdateOrderCommand, OrderModel>
{
    public async ValueTask<OrderModel> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FindAsync([command.Model.Id], cancellationToken: cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException();
        }
        command.Model.Adapt(order);
        await dbContext.Orders.AddAsync(order, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return order.Adapt<OrderModel>();
    }
}