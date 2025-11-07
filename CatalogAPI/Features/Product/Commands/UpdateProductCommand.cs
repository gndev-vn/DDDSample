using CatalogAPI.Domain;
using CatalogAPI.Features.Product.Models;
using Mapster;
using Mediator;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace CatalogAPI.Features.Product.Commands;

public record UpdateProductCommand(ProductModel Product) : IRequest<ProductModel>;

public class UpdateProductCommandHandler(AppDbContext dbContext) : IRequestHandler<UpdateProductCommand, ProductModel>
{
    /// <summary>
    /// Handles the update of a product entity within the database and maps the changes back to a product model.
    /// </summary>
    /// <param name="command">The command object containing the product model with updated values.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ProductModel"/> representing the updated product, or null if the update process failed.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the product to be updated cannot be found in the database.</exception>
    public async ValueTask<ProductModel> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.FindAsync([command.Product.Id], cancellationToken: cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException();
        }

        command.Product.Adapt(product);
        dbContext.Products.Update(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        return product.Adapt<ProductModel>();
    }
}