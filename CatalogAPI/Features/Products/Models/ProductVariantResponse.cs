using Shared.Models;
using Shared.ValueObjects;

namespace CatalogAPI.Features.Products.Models;

public class ProductVariantResponse : ModelBase
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal OverridePrice { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyCollection<VariantAttribute> Attributes { get; set; } = new List<VariantAttribute>();
}