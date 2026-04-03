using CatalogAPI.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Features.Products.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductResponse?>;

public class GetProductByIdQueryQueryHandler(AppDbContext dbContext)
    : IRequestHandler<GetProductByIdQuery, ProductResponse?>
{
    public async ValueTask<ProductResponse?> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.Where(x => x.Id == query.Id)
            .Select(x => new ProductResponse
            {
                Id = x.Id,
                CategoryId = EF.Property<Guid?>(x, "CategoryId") ?? Guid.Empty,
                Name = x.Name,
                BasePrice = x.BasePrice != null ? x.BasePrice.Amount : 0,
                Currency = x.BasePrice != null ? x.BasePrice.Currency : string.Empty,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                Slug = x.Slug,
                IsActive = x.IsActive,
            })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return product;
    }
}


