using CatalogAPI.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Features.ProductAttributes.GetProductAttributes;

public sealed record GetProductAttributesQuery : IRequest<List<ProductAttributeResponse>>;

public sealed class GetProductAttributesQueryHandler(AppDbContext dbContext)
    : IRequestHandler<GetProductAttributesQuery, List<ProductAttributeResponse>>
{
    public async ValueTask<List<ProductAttributeResponse>> Handle(GetProductAttributesQuery query, CancellationToken cancellationToken)
    {
        var usageCounts = await dbContext.ProductVariants
            .AsNoTracking()
            .SelectMany(variant => variant.Attributes)
            .GroupBy(attribute => attribute.AttributeId)
            .Select(group => new { AttributeId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.AttributeId, item => item.Count, cancellationToken);

        var definitions = await dbContext.ProductAttributeDefinitions
            .AsNoTracking()
            .OrderBy(definition => definition.Name)
            .ToListAsync(cancellationToken);

        return definitions
            .Select(definition => new ProductAttributeResponse
            {
                Id = definition.Id,
                Name = definition.Name,
                UsageCount = usageCounts.GetValueOrDefault(definition.Id),
            })
            .ToList();
    }
}
