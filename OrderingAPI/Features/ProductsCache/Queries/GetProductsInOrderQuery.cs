using Mediator;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Features.ProductsCache.Models;

namespace OrderingAPI.Features.ProductsCache.Queries;

public record GetProductsInOrderQuery(Guid Id) : IRequest<IEnumerable<ProductCacheModel>>;

public class GetProductsInOrderQueryHandler(AppDbContext dbContext) : IRequestHandler<GetProductsInOrderQuery, IEnumerable<ProductCacheModel>>
{
    public async ValueTask<IEnumerable<ProductCacheModel>> Handle(GetProductsInOrderQuery query, CancellationToken cancellationToken)
    {
        var products = await dbContext.OrderLines
            .Where(item => item.OrderId == query.Id)
            .Join(dbContext.ProductCaches,
                orderItem => orderItem.ProductId,
                product => product.Id,
                (orderItem, product) => new ProductCacheModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    CurrentPrice = product.CurrentPrice,
                    Currency = product.Currency,
                    IsActive = product.IsActive,
                    ImageUrl = product.ImageUrl
                })
            .ToListAsync(cancellationToken: cancellationToken);
        return products;
    }
}