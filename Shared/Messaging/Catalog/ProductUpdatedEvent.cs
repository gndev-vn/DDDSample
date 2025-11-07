using Wolverine.Attributes;

namespace Shared.Messaging.Catalog;

[Topic("catalog.product.updated")]

public class ProductUpdatedEvent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public string Slug { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}