using CatalogAPI.Domain;
using CatalogAPI.Features.Products.Models;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Features.Products.Queries;

public record GetProductVariantsByIdQuery(Guid Id) : IRequest<List<ProductVariantResponse>>;

public class GetProductVariantsByIdQueryHandler(AppDbContext dbContext)
    : IRequestHandler<GetProductVariantsByIdQuery, List<ProductVariantResponse>>
{
    /// <summary>
    /// Handles the query to retrieve a product by its identifier.
    /// </summary>
    /// <param name="query">The query object containing the product identifier.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A <see cref="ProductResponse"/> representing the product with the specified identifier,
    /// or null if no matching product is found.
    /// </returns>
    public async ValueTask<List<ProductVariantResponse>> Handle(GetProductVariantsByIdQuery query, CancellationToken cancellationToken)
    {
        var productVariants = await dbContext.ProductVariants
            .Where(x => x.ProductId == query.Id)
            .Select(x => new ProductVariantResponse
            {
                Id = x.Id,
                Name = x.Name,
                OverridePrice = x.OverridePrice.Amount,
                Currency = x.OverridePrice.Currency,
                Description = x.Description,
                IsActive = x.IsActive,
            }).ToListAsync(cancellationToken);
        return productVariants;
    }
}