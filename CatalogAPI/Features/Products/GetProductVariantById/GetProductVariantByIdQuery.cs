using CatalogAPI.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Features.Products.GetProductVariantById;

public sealed record GetProductVariantByIdQuery(Guid Id) : IRequest<ProductVariantResponse?>;

public sealed class GetProductVariantByIdQueryHandler(AppDbContext dbContext)
    : IRequestHandler<GetProductVariantByIdQuery, ProductVariantResponse?>
{
    public async ValueTask<ProductVariantResponse?> Handle(GetProductVariantByIdQuery query,
        CancellationToken cancellationToken)
    {
        return await dbContext.ProductVariants
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Select(ProductVariantMappings.Projection)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}

