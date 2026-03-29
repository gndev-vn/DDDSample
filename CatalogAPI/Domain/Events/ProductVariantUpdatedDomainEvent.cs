using Shared.Messaging.Catalog;
using Shared.Models;
using Wolverine;

namespace CatalogAPI.Domain.Events;

public sealed class ProductVariantUpdatedDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public bool IsActive { get; set; }
}

public sealed class ProductVariantUpdatedDomainEventHandler
{
    public static async Task Handle(ProductVariantUpdatedDomainEvent @event, IMessageBus bus,
        ILogger<ProductVariantUpdatedDomainEventHandler> logger)
    {
        logger.LogInformation("Received ProductVariantUpdated event for product variant {ProductVariantId}", @event.Id);
        await bus.PublishAsync(new ProductVariantUpdatedEvent
        {
            Id = @event.Id,
            ProductId = @event.ProductId,
            Sku = @event.Sku,
            Name = @event.Name,
            CurrentPrice = @event.CurrentPrice,
            Currency = @event.Currency,
            IsActive = @event.IsActive
        });
    }
}
