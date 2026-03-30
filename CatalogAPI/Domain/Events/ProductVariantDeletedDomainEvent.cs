using Shared.Messaging.Catalog;
using Shared.Models;
using Wolverine;

namespace CatalogAPI.Domain.Events;

public sealed class ProductVariantDeletedDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
}

public sealed class ProductVariantDeletedDomainEventHandler
{
    public static async Task Handle(ProductVariantDeletedDomainEvent @event, IMessageBus bus,
        ILogger<ProductVariantDeletedDomainEventHandler> logger)
    {
        logger.LogInformation("[CatalogAPI] Publishing ProductVariantDeletedEvent for product variant {ProductVariantId} ({Sku})",
            @event.Id, @event.Sku);
        await bus.PublishAsync(new ProductVariantDeletedEvent
        {
            Id = @event.Id,
            ProductId = @event.ProductId,
            Sku = @event.Sku
        });
    }
}
