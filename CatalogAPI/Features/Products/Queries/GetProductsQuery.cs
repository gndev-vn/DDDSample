using CatalogAPI.Domain;
using CatalogAPI.Features.Products.Models;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;

namespace CatalogAPI.Features.Products.Queries;

public record GetProductsQuery(string Name = "", string Slug = "", int Page = 1, int PageSize = 5)
    : IPagedQuery<List<ProductResponse>>;

public class GetProductsQueryHandler(AppDbContext dbContext) : IRequestHandler<GetProductsQuery, List<ProductResponse>>
{
    public async ValueTask<List<ProductResponse>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var productsQuery = dbContext.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            productsQuery = productsQuery.Where(product => product.Name.Contains(query.Name));
        }

        if (!string.IsNullOrWhiteSpace(query.Slug))
        {
            productsQuery = productsQuery.Where(product => product.Slug.Contains(query.Slug));
        }

        productsQuery = string.IsNullOrWhiteSpace(query.Slug)
            ? productsQuery.OrderBy(p => p.Name)
            : productsQuery.OrderBy(p => p.Slug);

        return await productsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(product => new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                BasePrice = product.BasePrice.Amount,
                Currency = product.BasePrice.Currency,
                IsActive = product.IsActive
            })
            .ToListAsync(cancellationToken: cancellationToken);
    }
}
