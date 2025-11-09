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
    /// <summary>
    /// Handles the GetCategoriesQuery by retrieving filtered, sorted, and paginated categories
    /// </summary>
    /// <param name="query">The query containing search and pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of category models matching the query criteria</returns>
    public async ValueTask<List<CategoryModel>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(dbContext);

        // Apply filtering
        var categoriesQuery = ApplyFilters(dbContext.Categories, query);

        // Apply sorting
        categoriesQuery = ApplySorting(categoriesQuery, query.Slug);

        // Apply pagination and projection
        var categories = await ApplyPaginationAndProjection(
            categoriesQuery,
            query.Page,
            query.PageSize);

        return categories;
    }

    private static IQueryable<Domain.Entities.Category> ApplyFilters(DbSet<Domain.Entities.Category> categories, GetCategoriesQuery query)
    {
        return categories.Where(p =>
            p.Name.Contains(query.Name) ||
            p.Slug.Contains(query.Slug));
    }

    private static IQueryable<Domain.Entities.Category> ApplySorting(IQueryable<Domain.Entities.Category> query, string searchSlug)
    {
        return string.IsNullOrEmpty(searchSlug)
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