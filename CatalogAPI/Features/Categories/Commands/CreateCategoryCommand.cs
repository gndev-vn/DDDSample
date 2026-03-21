using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Features.Categories.Models;
using Mapster;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;

namespace CatalogAPI.Features.Categories.Commands;

public record CreateCategoryCommand(CategoryCreateRequest Model) : IRequest<CategoryModel>;

public class CreateCategoryCommandHandler(AppDbContext dbContext)
    : IRequestHandler<CreateCategoryCommand, CategoryModel>
{
    /// <summary>
    /// Handles the creation of a new category by saving it to the database and returning the created category model.
    /// </summary>
    /// <param name="command">An instance of <see cref="CreateCategoryCommand"/> containing the data for the new category.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="CategoryModel"/> representing the created category, or null if the operation fails.</returns>
    public async ValueTask<CategoryModel> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        if (command.Model.ParentId != Guid.Empty)
        {
            var parentExists = await dbContext.Categories.AnyAsync(x => x.Id == command.Model.ParentId, cancellationToken);
            if (!parentExists)
            {
                throw new NotFoundException("Category", command.Model.ParentId);
            }
        }

        Guid? parentId = command.Model.ParentId == Guid.Empty ? null : command.Model.ParentId;
        var category = new Category(command.Model.Name, command.Model.Description, command.Model.Slug, parentId: parentId);
        await dbContext.Categories.AddAsync(category, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return category.Adapt<CategoryModel>();
    }
}
