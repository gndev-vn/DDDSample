using Mapster;
using Mediator;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using OrderingAPI.Features.Order.Models;
using Shared.Models;
using Shared.ValueObjects;

namespace OrderingAPI.Features.Order.Commands;

public record CreateOrderCommand(
    Guid CustomerId,
    List<OrderLineModel> Lines,
    AddressModel ShippingAddress,
    AddressModel? BillingAddress) : IRequest<OrderModel>;

public class CreateOrderCommandHandler(AppDbContext dbContext) : IRequestHandler<CreateOrderCommand, OrderModel>
{
    public async ValueTask<OrderModel> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var newOrder = Domain.Entities.Order.Create(command.CustomerId, command.Lines.Adapt<List<OrderLine>>(),
            command.ShippingAddress.Adapt<Address>()
        );
        await dbContext.Orders.AddAsync(newOrder, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return newOrder.Adapt<OrderModel>();
    }
}