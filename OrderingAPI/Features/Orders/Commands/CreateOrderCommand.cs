using Mapster;
using Mediator;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using OrderingAPI.Features.Orders.Models;
using Shared.Models;
using Shared.ValueObjects;

namespace OrderingAPI.Features.Orders.Commands;

public record CreateOrderCommand(
    Guid CustomerId,
    List<OrderLineModel> Lines,
    AddressModel ShippingAddress,
    AddressModel? BillingAddress) : IRequest<OrderModel>;

public class CreateOrderCommandHandler(AppDbContext dbContext) : IRequestHandler<CreateOrderCommand, OrderModel>
{
    public async ValueTask<OrderModel> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var newOrder = Order.Create(command.CustomerId, command.Lines.Select(ToOrderLine).ToList(),
            command.ShippingAddress.Adapt<Address>(),
            command.BillingAddress?.Adapt<Address>()
        );
        await dbContext.Orders.AddAsync(newOrder, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToOrderModel(newOrder);
    }

    private static OrderLine ToOrderLine(OrderLineModel model)
    {
        return new OrderLine(
            new Sku(model.Sku),
            Quantity.Of(model.Quantity),
            new Money(model.UnitPrice, model.Currency));
    }

    private static OrderModel ToOrderModel(Order order)
    {
        return new OrderModel
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status,
            ShippingAddress = order.ShippingAddress == null
                ? null
                : new AddressModel(
                    order.ShippingAddress.Line1,
                    order.ShippingAddress.Line2,
                    order.ShippingAddress.City,
                    order.ShippingAddress.Province,
                    order.ShippingAddress.District,
                    order.ShippingAddress.Ward),
            Lines = order.Lines.Select(line => new OrderLineModel
            {
                Id = line.Id,
                Sku = line.Sku.Value,
                Quantity = line.Quantity.Value,
                UnitPrice = line.Total.Amount,
                Currency = line.Total.Currency
            }).ToList()
        };
    }
}
