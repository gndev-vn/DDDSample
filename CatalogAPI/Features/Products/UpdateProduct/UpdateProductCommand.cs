using CatalogAPI.Domain;
using CatalogAPI.Features.Products.GetProductById;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using Shared.ValueObjects;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace CatalogAPI.Features.Products.UpdateProduct;

public record UpdateProductCommand(ProductUpdateRequest Product) : IRequest<ProductResponse>;

public class UpdateProductCommandHandler(AppDbContext dbContext) : IRequestHandler<UpdateProductCommand, ProductResponse>
{
    public async ValueTask<ProductResponse> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.FindAsync([command.Product.Id], cancellationToken: cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException();
        }

        var category = await dbContext.Categories
            .FirstOrDefaultAsync(x => x.Id == command.Product.CategoryId, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException("Category", command.Product.CategoryId);
        }

        product.UpdateDetails(
            command.Product.Name,
            command.Product.Description,
            command.Product.Slug,
            new Money(command.Product.BasePrice, command.Product.Currency));
        product.Category = category;
        dbContext.Products.Update(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new ProductResponse
        {
            Id = product.Id,
            CategoryId = category.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            BasePrice = product.BasePrice?.Amount ?? 0,
            Currency = product.BasePrice?.Currency ?? string.Empty,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive
        };
    }
}

