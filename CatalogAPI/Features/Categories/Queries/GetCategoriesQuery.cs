using CatalogAPI.Domain;
using CatalogAPI.Features.Categories.Models;
using Mapster;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;

namespace CatalogAPI.Features.Categories.Queries;

public record GetCategoriesQuery(string Name = "", string Slug = "", int Page = 1, int PageSize = 5)
    : IPagedQuery<List<CategoryModel>>;

public class GetCategoriesQueryHandler(AppDbContext dbContext)
    : IRequestHandler<GetCategoriesQuery, List<CategoryModel>>
{
    public async ValueTask<List<CategoryModel>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(dbContext);

        var categoriesQuery = ApplySorting(ApplyFilters(dbContext.Categories.AsNoTracking(), query), query.Slug);

        return await ApplyPaginationAndProjection(categoriesQuery, query.Page, query.PageSize);
    }

    private static IQueryable<Domain.Entities.Category> ApplyFilters(IQueryable<Domain.Entities.Category> categories,
        GetCategoriesQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            categories = categories.Where(category => category.Name.Contains(query.Name));
        }

        if (!string.IsNullOrWhiteSpace(query.Slug))
        {
            categories = categories.Where(category => category.Slug.Contains(query.Slug));
        }

        return categories;
    }

    private static IQueryable<Domain.Entities.Category> ApplySorting(IQueryable<Domain.Entities.Category> query, string searchSlug)
    {
        return string.IsNullOrWhiteSpace(searchSlug)
            ? query.OrderBy(p => p.Name)
            : query.OrderBy(p => p.Slug);
    }

    private static async Task<List<CategoryModel>> ApplyPaginationAndProjection(
        IQueryable<Domain.Entities.Category> query,
        int page,
        int pageSize)
    {
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectToType<CategoryModel>()
            .ToListAsync();
    }
}
