using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using Shared.Messaging.Catalog;

namespace OrderingAPI.Messaging;

[ScheduleRetry(typeof(DbUpdateException), 5, 15, 30)]
[RetryNow(typeof(TimeoutException), 50, 100, 250)]
public class ProductVariantCreatedEventConsumer
{
    public static async Task HandleAsync(ProductVariantCreatedEvent @event, ILogger<ProductVariantCreatedEventConsumer> logger,
        AppDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[OrderingAPI] Consuming ProductVariantCreatedEvent for variant {VariantId} ({Sku})",
            @event.Id,
            @event.Sku);

        if (await dbContext.ProductCaches.AnyAsync(x => x.Id == @event.Id || x.Sku == @event.Sku, cancellationToken))
        {
            logger.LogInformation(
                "[OrderingAPI] Product cache already exists for variant {VariantId}; skipping ProductVariantCreatedEvent",
                @event.Id);
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
            IsActive = @event.IsActive,
            LastUpdatedUtc = now,
            UpdatedAtUtc = now
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
