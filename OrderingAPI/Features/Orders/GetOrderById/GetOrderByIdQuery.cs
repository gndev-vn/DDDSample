using Mediator;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using Shared.Models;

namespace OrderingAPI.Features.Orders.GetOrderById;

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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
