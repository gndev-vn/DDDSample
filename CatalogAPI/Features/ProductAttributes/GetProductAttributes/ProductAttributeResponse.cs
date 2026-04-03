using Shared.Models;

namespace CatalogAPI.Features.ProductAttributes.GetProductAttributes;

public sealed class ProductAttributeResponse : ModelBase
{
    public string Name { get; set; } = string.Empty;

    public int UsageCount { get; set; }
}
