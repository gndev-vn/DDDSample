using Mediator;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Features.Order.Models;
using Shared.Models;

namespace OrderingAPI.Features.Order.Queries;

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
                Total = new MoneyModel { Amount = o.Total.Amount, Currency = o.Total.Currency },
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
                    UnitPrice = new MoneyModel
                    {
                        Amount = l.Price.Amount,
                        Currency = l.Price.Currency
                    }
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}