using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Features.Categories.GetCategoryById;
using Mapster;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;

namespace CatalogAPI.Features.Categories.CreateCategory;

public record CreateCategoryCommand(CategoryCreateRequest Model) : IRequest<CategoryModel>;

public class CreateCategoryCommandHandler(AppDbContext dbContext)
    : IRequestHandler<CreateCategoryCommand, CategoryModel>
{
    public async ValueTask<CategoryModel> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        if (command.Model.ParentId != null)
        {
            var parentExists = await dbContext.Categories.AnyAsync(x => x.Id == command.Model.ParentId, cancellationToken);
            if (!parentExists)
            {
                throw new NotFoundException("Category", command.Model.ParentId);
            }
        }

        var parentId = command.Model.ParentId;
        var category = new Category(command.Model.Name, command.Model.Description, command.Model.Slug, parentId: parentId);
        await dbContext.Categories.AddAsync(category, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return category.Adapt<CategoryModel>();
    }
}

