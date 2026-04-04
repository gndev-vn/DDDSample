using Wolverine.Attributes;
using Shared.Messaging;

namespace Shared.Messaging.Catalog;

[Topic(KafkaTopics.Catalog.ProductVariantUpdated)]
public sealed class ProductVariantUpdatedEvent
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public bool IsActive { get; set; }
}
