using CatalogAPI.Domain;
using CatalogAPI.Features.Product.Models;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace CatalogAPI.Features.Product.Queries;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductModel?>;

public class GetProductByIdQueryQueryHandler(AppDbContext dbContext)
    : IRequestHandler<GetProductByIdQuery, ProductModel?>
{
    /// <summary>
    /// Handles the query to retrieve a product by its identifier.
    /// </summary>
    /// <param name="query">The query object containing the product identifier.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ProductModel"/> representing the product with the specified identifier,
    /// or null if no matching product is found.
    /// </returns>
    public async ValueTask<ProductModel?> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.Where(x => x.Id == query.Id)
            .Select(x => new ProductModel
            {
                Id = x.Id,
                Name = x.Name,
                BasePrice = new MoneyModel { Amount = x.BasePrice.Amount, Currency = x.BasePrice.Currency },
                Description = x.Description,
                Slug = x.Slug,
                IsActive = x.IsActive,
            })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return product;
    }
}