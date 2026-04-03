using CatalogAPI.Domain;
using CatalogAPI.Features.Products.GetProductVariantById;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Features.Products.GetProductVariants;

public sealed record GetProductVariantsQuery(int Page = 1, int PageSize = 20) : IRequest<List<ProductVariantResponse>>;

public sealed class GetProductVariantsQueryHandler(AppDbContext dbContext)
    : IRequestHandler<GetProductVariantsQuery, List<ProductVariantResponse>>
{
    public async ValueTask<List<ProductVariantResponse>> Handle(GetProductVariantsQuery query,
        CancellationToken cancellationToken)
    {
        return await dbContext.ProductVariants
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ProductVariantMappings.Projection)
            .ToListAsync(cancellationToken);
    }
}

public record GetProductVariantsByIdQuery(Guid Id) : IRequest<List<ProductVariantResponse>>;

public class GetProductVariantsByIdQueryHandler(AppDbContext dbContext)
    : IRequestHandler<GetProductVariantsByIdQuery, List<ProductVariantResponse>>
{
    public async ValueTask<List<ProductVariantResponse>> Handle(GetProductVariantsByIdQuery query,
        CancellationToken cancellationToken)
    {
        return await dbContext.ProductVariants
            .AsNoTracking()
            .Where(x => x.ProductId == query.Id)
            .OrderBy(x => x.Name)
            .Select(ProductVariantMappings.Projection)
            .ToListAsync(cancellationToken);
    }
}

