using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Features.Products.Models;
using Mapster;
using Mediator;
using Shared.ValueObjects;

namespace CatalogAPI.Features.Products.Commands;

public record CreateProductVariantCommand(ProductVariantCreateRequest Model) : IRequest<ProductVariantResponse>;

public class CreateProductVariantCommandHandler(AppDbContext dbContext) : IRequestHandler<CreateProductVariantCommand, ProductVariantResponse>
{
    /// <summary>
    /// Handles the creation of a new product by adding it to the database and returning the created product model.
    /// </summary>
    /// <param name="command">The command containing the details of the product to be created.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ProductResponse"/> representing the created product, or null if the creation fails.</returns>
    public async ValueTask<ProductVariantResponse> Handle(CreateProductVariantCommand command, CancellationToken cancellationToken)
    {
        var productVariant = ProductVariant.Create(command.Model.Name, command.Model.Description, command.Model.Sku,
            new Money(command.Model.OverridePrice, command.Model.Currency), command.Model.Attributes);
        await dbContext.ProductVariants.AddAsync(productVariant, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return productVariant.Adapt<ProductVariantResponse>();
    }
}