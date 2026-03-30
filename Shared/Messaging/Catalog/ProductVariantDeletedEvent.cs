using Wolverine.Attributes;

namespace Shared.Messaging.Catalog;

[Topic("catalog.product-variant.deleted")]
public sealed class ProductVariantDeletedEvent
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
}
