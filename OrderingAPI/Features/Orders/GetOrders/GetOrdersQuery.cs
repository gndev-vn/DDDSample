using Mediator;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using Shared.Models;

namespace OrderingAPI.Features.Orders.GetOrders;

public sealed record GetOrdersQuery(Guid? CustomerId = null) : IRequest<List<OrderingAPI.Features.Orders.GetOrderById.OrderModel>>;

public class GetOrdersQueryHandler(AppDbContext dbContext) : IRequestHandler<GetOrdersQuery, List<OrderingAPI.Features.Orders.GetOrderById.OrderModel>>
{
    public async ValueTask<List<OrderingAPI.Features.Orders.GetOrderById.OrderModel>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = dbContext.Orders.AsNoTracking();
        if (request.CustomerId.HasValue)
        {
            orders = orders.Where(order => order.CustomerId == request.CustomerId.Value);
        }

        return await orders
            .Select(o => new OrderingAPI.Features.Orders.GetOrderById.OrderModel
            {
                Id = o.Id,
                Status = o.Status,
                CustomerId = o.CustomerId,
                CustomerName = o.CustomerName,
                CustomerEmail = o.CustomerEmail,
                CustomerPhone = o.CustomerPhone,
                ShippingAddress = o.ShippingAddress == null ? null : new AddressModel(
                    o.ShippingAddress.Line1,
                    string.IsNullOrWhiteSpace(o.ShippingAddress.Line2) ? null : o.ShippingAddress.Line2,
                    o.ShippingAddress.City,
                    o.ShippingAddress.Province,
                    o.ShippingAddress.District,
                    o.ShippingAddress.Ward),
                Lines = o.Lines.Select(l => new OrderingAPI.Features.Orders.GetOrderById.OrderLineModel
                {
                    ProductId = l.ProductId,
                    Name = dbContext.ProductCaches.Where(p => p.Id == l.ProductId).Select(p => p.Name).FirstOrDefault() ?? string.Empty,
                    Sku = l.Sku.Value,
                    Quantity = l.Quantity.Value,
                    UnitPrice = l.Total.Amount,
                    Currency = l.Total.Currency,
                }).ToList(),
            })
            .ToListAsync(cancellationToken);
    }
}

