using CatalogAPI.Domain;
using CatalogAPI.Features.Products.GetProductVariantById;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using Shared.ValueObjects;

namespace CatalogAPI.Features.Products.UpdateProductVariant;

public sealed record UpdateProductVariantCommand(ProductVariantUpdateRequest Model) : IRequest<ProductVariantResponse>;

public sealed class UpdateProductVariantCommandHandler(AppDbContext dbContext)
    : IRequestHandler<UpdateProductVariantCommand, ProductVariantResponse>
{
    public async ValueTask<ProductVariantResponse> Handle(UpdateProductVariantCommand command, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .Include(x => x.Variants)
            .FirstOrDefaultAsync(x => x.Id == command.Model.ParentId, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException("Product", command.Model.ParentId);
        }

        var resolvedAttributes = await ResolveAttributes(command.Model.Attributes, cancellationToken);
        var overridePrice = command.Model.OverridePrice > 0
            ? new Money(command.Model.OverridePrice, command.Model.Currency)
            : null;

        var productVariant = product.UpdateVariant(
            command.Model.Id,
            command.Model.Name,
            command.Model.Sku,
            command.Model.Description,
            overridePrice,
            resolvedAttributes);

        await dbContext.SaveChangesAsync(cancellationToken);
        return ProductVariantMappings.ToResponse(productVariant);
    }

    private async Task<List<VariantAttribute>> ResolveAttributes(
        IReadOnlyCollection<ProductVariantAttributeValueRequest> requests,
        CancellationToken cancellationToken)
    {
        var attributeIds = requests.Select(attribute => attribute.AttributeId).Distinct().ToList();
        var definitions = await dbContext.ProductAttributeDefinitions
            .Where(definition => attributeIds.Contains(definition.Id))
            .ToDictionaryAsync(definition => definition.Id, cancellationToken);

        if (definitions.Count != attributeIds.Count)
        {
            var missingId = attributeIds.First(id => !definitions.ContainsKey(id));
            throw new NotFoundException("ProductAttribute", missingId);
        }

        return requests
            .Select(request => new VariantAttribute(
                request.AttributeId,
                definitions[request.AttributeId].Name,
                request.Value))
            .ToList();
    }
}
