using Mediator;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Features.Orders.Models;
using Shared.Models;

namespace OrderingAPI.Features.Orders.Queries;

public record GetOrdersQuery : IRequest<IEnumerable<OrderModel>>;

public class GetOrdersQueryHandler(AppDbContext dbContext) : IRequestHandler<GetOrdersQuery, IEnumerable<OrderModel>>
{
    public async ValueTask<IEnumerable<OrderModel>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Select(o => new OrderModel
            {
                Id = o.Id,
                ShippingAddress = new AddressModel(
                    o.ShippingAddress.Line1,
                    string.IsNullOrWhiteSpace(o.ShippingAddress.Line2)
                        ? null
                        : o.ShippingAddress.Line2,
                    o.ShippingAddress.City,
                    o.ShippingAddress.Province,
                    o.ShippingAddress.District,
                    o.ShippingAddress.Ward),
                Lines = o.Lines.Select(l => new OrderLineModel
                {
                    Sku = l.Sku.Value,
                    Quantity = l.Quantity.Value,
                    UnitPrice = l.Total.Amount,
                    Currency = l.Total.Currency
                }).ToList()
            })
            .ToListAsync(cancellationToken: cancellationToken);
    }
}