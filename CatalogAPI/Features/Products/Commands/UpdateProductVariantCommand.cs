using CatalogAPI.Domain;
using CatalogAPI.Features.Products.Models;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using Shared.ValueObjects;

namespace CatalogAPI.Features.Products.Commands;

public sealed record UpdateProductVariantCommand(ProductVariantUpdateRequest Model) : IRequest<ProductVariantResponse>;

public sealed class UpdateProductVariantCommandHandler(AppDbContext dbContext)
    : IRequestHandler<UpdateProductVariantCommand, ProductVariantResponse>
{
    public async ValueTask<ProductVariantResponse> Handle(UpdateProductVariantCommand command,
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

        var productVariant = product.UpdateVariant(
            command.Model.Id,
            command.Model.Name,
            command.Model.Sku,
            command.Model.Description,
            overridePrice);

        await dbContext.SaveChangesAsync(cancellationToken);
        return ProductVariantMappings.ToResponse(productVariant);
    }
}
