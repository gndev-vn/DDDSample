using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Features.Products.Models;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using Shared.ValueObjects;

namespace CatalogAPI.Features.Products.Commands;

public record CreateProductCommand(Features.Products.Models.ProductCreateRequest Model) : IRequest<ProductResponse>;

public class CreateProductCommandHandler(AppDbContext dbContext) : IRequestHandler<CreateProductCommand, ProductResponse>
{
    /// <summary>
    /// Handles the creation of a new product by adding it to the database and returning the created product model.
    /// </summary>
    /// <param name="command">The command containing the details of the product to be created.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ProductResponse"/> representing the created product, or null if the creation fails.</returns>
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
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            BasePrice = product.BasePrice?.Amount ?? 0,
            Currency = product.BasePrice?.Currency ?? string.Empty,
            IsActive = product.IsActive
        };
    }
}
