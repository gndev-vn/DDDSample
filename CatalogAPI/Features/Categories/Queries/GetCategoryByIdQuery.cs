using CatalogAPI.Domain;
using CatalogAPI.Features.Categories.Models;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Features.Categories.Queries;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryModel?>;

public class GetCategoryByIdQueryHandler(AppDbContext dbContext) : IRequestHandler<GetCategoryByIdQuery, CategoryModel?>
{
    /// <summary>
    /// Handles the request to retrieve a specific category by its unique identifier.
    /// </summary>
    /// <param name="query">The query containing the identifier of the category to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the category model
    /// corresponding to the specified identifier, or null if no such category exists.
    /// </returns>
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
            })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return category;
    }
}