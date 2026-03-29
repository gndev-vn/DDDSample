using Wolverine.Attributes;

namespace Shared.Messaging.Catalog;

[Topic("catalog.product.created")]
public class ProductCreatedEvent
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public long Version { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
