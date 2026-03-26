using CatalogAPI.Domain;
using Mediator;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace CatalogAPI.Features.Categories.Commands;

public record DeleteCategoryCommand(Guid Id) : IRequest<bool>;

public class DeleteCategoryCommandHandler(AppDbContext dbContext) : IRequestHandler<DeleteCategoryCommand, bool>
{
    /// <summary>
    /// Handles the process of deleting a category from the database.
    /// </summary>
    /// <param name="command">The command containing the ID of the category to be deleted.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a boolean indicating
    /// whether the operation was successful. Returns <c>true</c> if the category was deleted successfully, otherwise <c>false</c>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the category with the specified ID does not exist.</exception>
    public async ValueTask<bool> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.FindAsync(command.Id, cancellationToken: cancellationToken);
        if (category == null)
        {
            throw new KeyNotFoundException("Invalid category id");
        }

        dbContext.Categories.Remove(category);
        return await dbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}
