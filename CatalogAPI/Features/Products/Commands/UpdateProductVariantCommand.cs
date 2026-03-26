using CatalogAPI.Domain;
using CatalogAPI.Features.Products.Models;
using Mapster;
using Mediator;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace CatalogAPI.Features.Products.Commands;

public sealed record UpdateProductVariantCommand(ProductVariantUpdateRequest Model) : IRequest<ProductVariantResponse>;

public sealed class UpdateProductVariantCommandHandler(AppDbContext dbContext)
    : IRequestHandler<UpdateProductVariantCommand, ProductVariantResponse>
{
    public async ValueTask<ProductVariantResponse> Handle(UpdateProductVariantCommand command,
        CancellationToken cancellationToken)
    {
        var productVariant =
            await dbContext.ProductVariants.FindAsync([command.Model.Id], cancellationToken: cancellationToken);
        if (productVariant == null)
        {
            throw new KeyNotFoundException();
        }

        command.Model.Adapt(productVariant);
        dbContext.ProductVariants.Update(productVariant);
        await dbContext.SaveChangesAsync(cancellationToken);
        return productVariant.Adapt<ProductVariantResponse>();
    }
}