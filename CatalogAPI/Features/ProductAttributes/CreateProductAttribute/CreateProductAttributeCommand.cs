using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Features.ProductAttributes.GetProductAttributes;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;

namespace CatalogAPI.Features.ProductAttributes.CreateProductAttribute;

public sealed record CreateProductAttributeCommand(ProductAttributeCreateRequest Model) : IRequest<ProductAttributeResponse>;

public sealed class CreateProductAttributeCommandHandler(AppDbContext dbContext)
    : IRequestHandler<CreateProductAttributeCommand, ProductAttributeResponse>
{
    public async ValueTask<ProductAttributeResponse> Handle(CreateProductAttributeCommand command, CancellationToken cancellationToken)
    {
        var normalizedName = command.Model.Name.Trim();
        var exists = await dbContext.ProductAttributeDefinitions
            .AnyAsync(definition => definition.Name.ToUpper() == normalizedName.ToUpper(), cancellationToken);
        if (exists)
        {
            throw new BusinessRuleException($"Product attribute '{normalizedName}' already exists.");
        }

        var definition = new ProductAttributeDefinition(normalizedName);
        dbContext.ProductAttributeDefinitions.Add(definition);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ProductAttributeResponse
        {
            Id = definition.Id,
            Name = definition.Name,
            UsageCount = 0,
        };
    }
}
