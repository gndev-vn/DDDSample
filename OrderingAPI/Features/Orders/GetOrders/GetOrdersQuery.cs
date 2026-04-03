using Mediator;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Features.Orders.GetOrderById;
using Shared.Models;

namespace OrderingAPI.Features.Orders.GetOrders;

public record GetOrdersQuery : IRequest<List<OrderModel>>;

public class GetOrdersQueryHandler(AppDbContext dbContext) : IRequestHandler<GetOrdersQuery, List<OrderModel>>
{
    public async ValueTask<List<OrderModel>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Select(o => new OrderModel
            {
                Id = o.Id,
                Status = o.Status,
                CustomerId = o.CustomerId,
                ShippingAddress = o.ShippingAddress == null ? null : new AddressModel(
                    o.ShippingAddress.Line1,
                    string.IsNullOrWhiteSpace(o.ShippingAddress.Line2) ? null : o.ShippingAddress.Line2,
                    o.ShippingAddress.City,
                    o.ShippingAddress.Province,
                    o.ShippingAddress.District,
                    o.ShippingAddress.Ward),
                Lines = o.Lines.Select(l => new OrderLineModel
                {
                    ProductId = l.ProductId,
                    Name = dbContext.ProductCaches.Where(p => p.Id == l.ProductId).Select(p => p.Name).FirstOrDefault() ?? string.Empty,
                    Sku = l.Sku.Value,
                    Quantity = l.Quantity.Value,
                    UnitPrice = l.Total.Amount,
                    Currency = l.Total.Currency
                }).ToList()
            })
            .ToListAsync(cancellationToken: cancellationToken);
    }
}