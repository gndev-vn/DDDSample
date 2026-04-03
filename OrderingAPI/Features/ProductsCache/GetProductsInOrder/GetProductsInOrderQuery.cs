using Mediator;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;

namespace OrderingAPI.Features.ProductsCache.GetProductsInOrder;

public record GetProductsInOrderQuery(Guid Id) : IRequest<List<ProductCacheModel>>;

public class GetProductsInOrderQueryHandler(AppDbContext dbContext) : IRequestHandler<GetProductsInOrderQuery, List<ProductCacheModel>>
{
    public async ValueTask<List<ProductCacheModel>> Handle(GetProductsInOrderQuery query, CancellationToken cancellationToken)
    {
        return await dbContext.OrderLines
            .Where(item => item.OrderId == query.Id)
            .Join(dbContext.ProductCaches,
                orderItem => orderItem.ProductId,
                product => product.Id,
                (orderItem, product) => new ProductCacheModel
                {
                    Id = product.Id,
                    Sku = product.Sku,
                    Name = product.Name,
                    CurrentPrice = product.CurrentPrice,
                    Currency = product.Currency,
                    IsActive = product.IsActive,
                    ImageUrl = product.ImageUrl,
                    LastUpdatedUtc = product.LastUpdatedUtc
                })
            .ToListAsync(cancellationToken: cancellationToken);
    }
}