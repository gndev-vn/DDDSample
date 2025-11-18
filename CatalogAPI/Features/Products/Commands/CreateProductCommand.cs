using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Features.Products.Models;
using Mapster;
using Mediator;
using Shared.ValueObjects;

namespace CatalogAPI.Features.Products.Commands;

public record CreateProductCommand(Features.Products.Models.ProductCreateRequest Model) : IRequest<ProductModel>;

public class CreateProductCommandHandler(AppDbContext dbContext) : IRequestHandler<CreateProductCommand, ProductModel>
{
    /// <summary>
    /// Handles the creation of a new product by adding it to the database and returning the created product model.
    /// </summary>
    /// <param name="command">The command containing the details of the product to be created.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ProductModel"/> representing the created product, or null if the creation fails.</returns>
    public async ValueTask<ProductModel> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product(command.Model.Name, command.Model.Description, command.Model.Slug,
            command.Model.BasePrice.Adapt<Money>());
        await dbContext.Products.AddAsync(product, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return product.Adapt<ProductModel>();
    }
}