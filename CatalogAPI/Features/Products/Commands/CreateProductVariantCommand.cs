using CatalogAPI.Domain;
using CatalogAPI.Features.Products.Models;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using Shared.ValueObjects;

namespace CatalogAPI.Features.Products.Commands;

public record CreateProductVariantCommand(ProductVariantCreateRequest Model) : IRequest<ProductVariantResponse>;

public class CreateProductVariantCommandHandler(AppDbContext dbContext)
    : IRequestHandler<CreateProductVariantCommand, ProductVariantResponse>
{
    public async ValueTask<ProductVariantResponse> Handle(CreateProductVariantCommand command,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .Include(x => x.Variants)
            .FirstOrDefaultAsync(x => x.Id == command.Model.ParentId, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException("Product", command.Model.ParentId);
        }

        var overridePrice = command.Model.OverridePrice > 0
            ? new Money(command.Model.OverridePrice, command.Model.Currency)
            : null;

        var productVariant = product.AddVariant(
            command.Model.Name,
            command.Model.Sku,
            command.Model.Description,
            overridePrice,
            command.Model.Attributes);

        dbContext.ProductVariants.Add(productVariant);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ProductVariantMappings.ToResponse(productVariant);
    }
}
