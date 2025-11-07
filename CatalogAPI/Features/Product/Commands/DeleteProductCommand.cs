using CatalogAPI.Domain;
using Mediator;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace CatalogAPI.Features.Product.Commands;

public record DeleteProductCommand(Guid Id) : IRequest<bool>;

public class DeleteProductCommandHandler(AppDbContext dbContext) : IRequestHandler<DeleteProductCommand, bool>
{
    /// <summary>
    /// Handles the deletion of a product from the database.
    /// </summary>
    /// <param name="command">The command containing the ID of the product to be deleted.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the deletion was successful.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the product with the specified ID is not found in the database.</exception>
    public async ValueTask<bool> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.FindAsync([command.Id], cancellationToken: cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException("Invalid product id");
        }

        dbContext.Products.Remove(product);
        return await dbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}