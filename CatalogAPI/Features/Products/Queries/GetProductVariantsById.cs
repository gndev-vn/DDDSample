using CatalogAPI.Domain;
using CatalogAPI.Features.Products.Models;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Features.Products.Queries;

public record GetProductVariantsByIdQuery(Guid Id) : IRequest<List<ProductVariantResponse>>;

public class GetProductVariantsByIdQueryHandler(AppDbContext dbContext)
    : IRequestHandler<GetProductVariantsByIdQuery, List<ProductVariantResponse>>
{
    public async ValueTask<List<ProductVariantResponse>> Handle(GetProductVariantsByIdQuery query,
        CancellationToken cancellationToken)
    {
        return await dbContext.ProductVariants
            .Where(x => x.ProductId == query.Id)
            .OrderBy(x => x.Name)
            .Select(ProductVariantMappings.Projection)
            .ToListAsync(cancellationToken);
    }
}
