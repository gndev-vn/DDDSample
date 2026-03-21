using CatalogAPI.Domain;
using CatalogAPI.Features.Categories.Models;
using Mapster;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace CatalogAPI.Features.Categories.Commands;

public record UpdateCategoryCommand(CategoryUpdateRequest Model) : IRequest<CategoryModel>;

public class UpdateCategoryCommandHandler(AppDbContext dbContext)
    : IRequestHandler<UpdateCategoryCommand, CategoryModel>
{
    /// <summary>
    /// Handles the update of a category by adapting and saving updated data within the database.
    /// </summary>
    /// <param name="command">The command containing the updated category model data.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated category model after persisting the changes; or null if the category was not found.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified category does not exist in the database.</exception>
    public async ValueTask<CategoryModel> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.FindAsync([command.Model.Id], cancellationToken: cancellationToken);
        if (category == null)
        {
            throw new KeyNotFoundException();
        }

        if (command.Model.ParentId != Guid.Empty)
        {
            var parentExists = await dbContext.Categories.AnyAsync(x => x.Id == command.Model.ParentId, cancellationToken);
            if (!parentExists)
            {
                throw new NotFoundException("Category", command.Model.ParentId);
            }
        }

        command.Model.Adapt(category);
        category.ParentId = command.Model.ParentId == Guid.Empty ? null : command.Model.ParentId;
        dbContext.Categories.Update(category);
        await dbContext.SaveChangesAsync(cancellationToken);
        return category.Adapt<CategoryModel>();
    }
}
