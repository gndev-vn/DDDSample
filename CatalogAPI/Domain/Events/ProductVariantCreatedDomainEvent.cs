using Shared.Messaging.Catalog;
using Shared.Models;
using Wolverine;

namespace CatalogAPI.Domain.Events;

public sealed class ProductVariantCreatedDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public bool IsActive { get; set; }
}

public sealed class ProductVariantCreatedDomainEventHandler
{
    public static async Task Handle(ProductVariantCreatedDomainEvent @event, IMessageBus bus,
        ILogger<ProductVariantCreatedDomainEventHandler> logger)
    {
        logger.LogInformation("[CatalogAPI] Publishing ProductVariantCreatedEvent for product variant {ProductVariantId} ({Sku})",
            @event.Id, @event.Sku);
        await bus.PublishAsync(new ProductVariantCreatedEvent
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
