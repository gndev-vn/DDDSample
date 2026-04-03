using CatalogAPI.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Features.Categories.GetCategoryById;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryModel?>;

public class GetCategoryByIdQueryHandler(AppDbContext dbContext) : IRequestHandler<GetCategoryByIdQuery, CategoryModel?>
{
    public async ValueTask<CategoryModel?> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.Where(x => x.Id == query.Id)
            .Select(x => new CategoryModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Slug = x.Slug,
                IsActive = x.IsActive,
                ParentId = x.ParentId,
            })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return category;
    }
}

