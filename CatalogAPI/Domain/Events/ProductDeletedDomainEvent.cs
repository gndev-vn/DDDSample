using Shared.Messaging.Catalog;
using Shared.Models;
using Wolverine;

namespace CatalogAPI.Domain.Events;

public sealed class ProductDeletedDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
}

public sealed class ProductDeletedDomainEventHandler
{
    public static async Task HandleAsync(ProductDeletedDomainEvent @event, IMessageBus bus,
        ILogger<ProductDeletedDomainEventHandler> logger)
    {
        logger.LogInformation("[CatalogAPI] Publishing ProductDeletedEvent for product {ProductId}", @event.Id);
        await bus.PublishAsync(new ProductDeletedEvent { Id = @event.Id });
    }
}
