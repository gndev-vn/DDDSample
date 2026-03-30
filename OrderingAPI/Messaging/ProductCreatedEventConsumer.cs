using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using Shared.Messaging.Catalog;

namespace OrderingAPI.Messaging;

public class ProductCreatedEventConsumer
{
    public static async Task HandleAsync(ProductCreatedEvent @event, ILogger<ProductCreatedEventConsumer> logger,
        AppDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("[OrderingAPI] Consuming ProductCreatedEvent for product {ProductId} ({Sku})", @event.Id, @event.Sku);
        if (await dbContext.ProductCaches.AnyAsync(x => x.Id == @event.Id || x.Sku == @event.Sku, cancellationToken))
        {
            logger.LogInformation("[OrderingAPI] Product cache already exists for product {ProductId}; skipping ProductCreatedEvent", @event.Id);
            return;
        }

        var now = DateTime.UtcNow;
        await dbContext.ProductCaches.AddAsync(new ProductCache
        {
            Id = @event.Id,
            Sku = @event.Sku,
            Name = @event.Name,
            CurrentPrice = @event.CurrentPrice,
            Currency = @event.Currency,
            ImageUrl = @event.ImageUrl,
            IsActive = @event.IsActive,
            LastUpdatedUtc = now,
            UpdatedAtUtc = now
        }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
