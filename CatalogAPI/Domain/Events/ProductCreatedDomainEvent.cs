using Shared.Messaging.Catalog;
using Shared.Models;
using Wolverine;

namespace CatalogAPI.Domain.Events;

public class ProductCreatedDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public string Slug { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}

public class ProductCreatedDomainEventHandler
{
    public static async Task Handle(ProductCreatedDomainEvent @event, IMessageBus bus,
        ILogger<ProductCreatedDomainEventHandler> logger)
    {
        logger.LogInformation("Received ProductCreated event for product {ProductId}", @event.Id);
        await bus.PublishAsync(new ProductCreatedEvent
        {
            Id = @event.Id,
            Name = @event.Name,
            CurrentPrice = @event.CurrentPrice,
            Currency = @event.Currency,
            Slug = @event.Slug,
            ImageUrl = @event.ImageUrl
        });
    }
}