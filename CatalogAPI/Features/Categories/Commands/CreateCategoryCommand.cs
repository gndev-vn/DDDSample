using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Features.Categories.Models;
using Mapster;
using Mediator;

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
        var category = new Category(command.Model.Name, command.Model.Description, command.Model.Slug);
        await dbContext.Categories.AddAsync(category, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return category.Adapt<CategoryModel>();
    }
}