using CatalogAPI.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace CatalogAPI.Features.Categories.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : IRequest<bool>;

public class DeleteCategoryCommandHandler(AppDbContext dbContext) : IRequestHandler<DeleteCategoryCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (category == null)
        {
            throw new KeyNotFoundException("Invalid category id");
        }

        var hasChildren = await dbContext.Categories
            .IgnoreQueryFilters()
            .AnyAsync(x => x.ParentId == command.Id, cancellationToken);
        if (hasChildren)
        {
            throw new BusinessRuleException("Cannot delete a category that still has child categories.");
        }

        var hasProducts = await dbContext.Products
            .AnyAsync(x => EF.Property<Guid?>(x, "CategoryId") == command.Id, cancellationToken);
        if (hasProducts)
        {
            throw new BusinessRuleException("Cannot delete a category that still has products.");
        }

        category.MarkDeleted();
        dbContext.Categories.Remove(category);
        return await dbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}

