using CatalogAPI.Domain;
using CatalogAPI.Features.Product.Models;
using Mapster;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Common;

namespace CatalogAPI.Features.Product.Queries;

public record GetProductsQuery(string Name = "", string Slug = "", int Page = 1, int PageSize = 5)
    : IPagedQuery<List<ProductModel>>;

public class GetProductsQueryHandler(AppDbContext dbContext) : IRequestHandler<GetProductsQuery, List<ProductModel>>
{
    /// <summary>
    /// Handles the retrieval of products based on the provided query and database context.
    /// </summary>
    /// <param name="query">The query object containing pagination details and search filters such as name and slug.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation, containing a list of products that match the query.</returns>
    public async ValueTask<List<ProductModel>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var productsQuery = dbContext.Products
            .Where(p => p.Name.Contains(query.Name) || p.Slug.Contains(query.Slug));
        productsQuery = query.Slug.Length > 0
            ? productsQuery.OrderBy(p => p.Slug)
            : productsQuery.OrderBy(p => p.Name);

        var products = await productsQuery.Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectToType<ProductModel>()
            .ToListAsync(cancellationToken: cancellationToken);
        return products;
    }
}