using Shared.Messaging.Catalog;
using Shared.Models;
using Wolverine;

namespace CatalogAPI.Domain.Events;

public class ProductUpdatedDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public string Slug { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}

public class ProductUpdatedDomainEventHandler
{
    public async Task HandleAsync(ProductUpdatedDomainEvent @event, IMessageBus bus,
        ILogger<ProductUpdatedDomainEventHandler> logger)
    {
        logger.LogInformation("Product {Id} updated", @event.Id);
        await bus.PublishAsync(new ProductUpdatedEvent
        {
            Id = @event.Id,
            Name = @event.Name,
            CurrentPrice = @event.CurrentPrice,
            Currency = @event.Currency,
            Slug = @event.Slug,
            ImageUrl = @event.ImageUrl,
        });
    }
}