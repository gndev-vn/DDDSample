using Mediator;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Features.Orders.Models;
using Shared.Models;

namespace OrderingAPI.Features.Orders.Queries;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderModel?>;

public class GetOrderByIdQueryHandler(AppDbContext dbContext) : IRequestHandler<GetOrderByIdQuery, OrderModel?>
{
    public async ValueTask<OrderModel?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == request.Id)
            .Select(o => new OrderModel
            {
                Id = o.Id,
                Status = o.Status,
                CustomerId = o.CustomerId,
                ShippingAddress = o.ShippingAddress == null ? null : new AddressModel(
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
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}
