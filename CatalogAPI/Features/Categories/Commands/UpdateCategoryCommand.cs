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
    public async ValueTask<CategoryModel> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == command.Model.Id, cancellationToken);
        if (category == null)
        {
            throw new KeyNotFoundException();
        }

        Guid? parentId = command.Model.ParentId == Guid.Empty ? null : command.Model.ParentId;
        if (parentId.HasValue)
        {
            var parentExists = await dbContext.Categories.AnyAsync(x => x.Id == parentId.Value, cancellationToken);
            if (!parentExists)
            {
                throw new NotFoundException("Category", parentId.Value);
            }

            await EnsureNoHierarchyCycleAsync(category.Id, parentId.Value, cancellationToken);
        }

        category.Update(
            command.Model.Name,
            command.Model.Description,
            command.Model.Slug,
            command.Model.IsActive ?? category.IsActive,
            parentId);

        await dbContext.SaveChangesAsync(cancellationToken);
        return category.Adapt<CategoryModel>();
    }

    private async Task EnsureNoHierarchyCycleAsync(Guid categoryId, Guid parentId, CancellationToken cancellationToken)
    {
        var visited = new HashSet<Guid>();
        Guid? currentParentId = parentId;

        while (currentParentId.HasValue)
        {
            if (currentParentId.Value == categoryId || !visited.Add(currentParentId.Value))
            {
                throw new BusinessRuleException("Category hierarchy cannot contain cycles.");
            }

            currentParentId = await dbContext.Categories
                .IgnoreQueryFilters()
                .Where(x => x.Id == currentParentId.Value)
                .Select(x => x.ParentId)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
