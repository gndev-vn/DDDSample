using CatalogAPI.Domain;
using CatalogAPI.Features.Products.Models;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Features.Products.Queries;

public sealed record GetProductVariantsQuery(int Page = 1, int PageSize = 20) : IRequest<List<ProductVariantResponse>>;

public sealed class GetProductVariantsQueryHandler(AppDbContext dbContext)
    : IRequestHandler<GetProductVariantsQuery, List<ProductVariantResponse>>
{
    public async ValueTask<List<ProductVariantResponse>> Handle(GetProductVariantsQuery query,
        CancellationToken cancellationToken)
    {
        return await dbContext.ProductVariants
            .OrderBy(x => x.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ProductVariantMappings.Projection)
            .ToListAsync(cancellationToken);
    }
}
