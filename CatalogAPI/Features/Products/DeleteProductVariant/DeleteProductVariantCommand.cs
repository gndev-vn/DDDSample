using CatalogAPI.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace CatalogAPI.Features.Products.DeleteProductVariant;

public sealed record DeleteProductVariantCommand(Guid Id) : IRequest<bool>;

public sealed class DeleteProductVariantCommandHandler(AppDbContext dbContext)
    : IRequestHandler<DeleteProductVariantCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteProductVariantCommand command, CancellationToken cancellationToken)
    {
        var variantOwner = await dbContext.ProductVariants
            .AsNoTracking()
            .Where(x => x.Id == command.Id)
            .Select(x => new { x.Id, x.ProductId })
            .FirstOrDefaultAsync(cancellationToken);
        if (variantOwner is null)
        {
            throw new KeyNotFoundException("Invalid product variant id");
        }

        var product = await dbContext.Products
            .Include(x => x.Variants)
            .FirstOrDefaultAsync(x => x.Id == variantOwner.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException("Invalid product id");

        var removedVariant = product.RemoveVariant(command.Id);
        dbContext.ProductVariants.Remove(removedVariant);

        return await dbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}

