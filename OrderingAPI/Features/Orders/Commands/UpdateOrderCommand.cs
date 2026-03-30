using Mapster;
using Mediator;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using OrderingAPI.Features.Orders.Models;
using Shared.Models;
using Shared.ValueObjects;

namespace OrderingAPI.Features.Orders.Commands;

public record UpdateOrderCommand(OrderModel Model) : IRequest<OrderModel>;

public class UpdateOrderCommandHandler(AppDbContext dbContext) : IRequestHandler<UpdateOrderCommand, OrderModel>
{
    public async ValueTask<OrderModel> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await LoadOrderAsync(command.Model.Id, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException();
        }

        var shippingAddress = command.Model.ShippingAddress?.Adapt<Address>()
            ?? throw new ArgumentException("Shipping address is required");
        var lines = command.Model.Lines.Select(ToOrderLine).ToList();
        var existingLines = order.Lines.ToList();

        if (existingLines.Count != 0)
        {
            dbContext.OrderLines.RemoveRange(existingLines);
            await dbContext.SaveChangesAsync(cancellationToken);

            dbContext.ChangeTracker.Clear();
            order = await LoadOrderAsync(command.Model.Id, cancellationToken);
            if (order == null)
            {
                throw new KeyNotFoundException();
            }
        }

        order.Update(shippingAddress, lines);
        dbContext.Entry(order).State = EntityState.Modified;
        foreach (var line in order.Lines)
        {
            dbContext.Entry(line).State = EntityState.Added;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToOrderModel(order);
    }

    private static OrderLine ToOrderLine(OrderLineModel model)
    {
        return new OrderLine(
            new Sku(model.Sku),
            Quantity.Of(model.Quantity),
            new Money(model.UnitPrice, model.Currency));
    }

    private async Task<Order?> LoadOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
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
