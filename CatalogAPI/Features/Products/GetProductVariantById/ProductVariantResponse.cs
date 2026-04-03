using Shared.Models;

namespace CatalogAPI.Features.Products.GetProductVariantById;

public sealed class ProductVariantAttributeResponse
{
    public Guid AttributeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}

public class ProductVariantResponse : ModelBase
{
    public Guid ParentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public decimal OverridePrice { get; set; }

    public bool IsActive { get; set; }

    public IReadOnlyCollection<ProductVariantAttributeResponse> Attributes { get; set; } = new List<ProductVariantAttributeResponse>();
}
