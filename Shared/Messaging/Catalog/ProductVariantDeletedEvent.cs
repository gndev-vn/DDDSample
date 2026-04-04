using Wolverine.Attributes;
using Shared.Messaging;

namespace Shared.Messaging.Catalog;

[Topic(KafkaTopics.Catalog.ProductVariantDeleted)]
public sealed class ProductVariantDeletedEvent
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
}
