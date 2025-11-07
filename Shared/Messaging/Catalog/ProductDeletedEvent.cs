using Wolverine.Attributes;

namespace Shared.Messaging.Catalog;

[Topic("catalog.product.deleted")]
public class ProductDeletedEvent
{
    public Guid Id { get; set; }
}