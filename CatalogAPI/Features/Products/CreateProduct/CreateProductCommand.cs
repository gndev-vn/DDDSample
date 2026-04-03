using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Features.Products.GetProductById;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using Shared.ValueObjects;

namespace CatalogAPI.Features.Products.CreateProduct;

public record CreateProductCommand(ProductCreateRequest Model) : IRequest<ProductResponse>;

public class CreateProductCommandHandler(AppDbContext dbContext) : IRequestHandler<CreateProductCommand, ProductResponse>
{
    public async ValueTask<ProductResponse> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(x => x.Id == command.Model.CategoryId, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException("Category", command.Model.CategoryId);
        }

        var product = new Product(command.Model.Name, command.Model.Description, command.Model.Slug,
            new Money(command.Model.BasePrice, command.Model.Currency));
        product.Category = category;
        await dbContext.Products.AddAsync(product, cancellationToken);
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

