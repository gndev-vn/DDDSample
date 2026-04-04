using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using Shared.Messaging.Catalog;

namespace OrderingAPI.Messaging;

[ScheduleRetry(typeof(DbUpdateException), 5, 15, 30)]
[RetryNow(typeof(TimeoutException), 50, 100, 250)]
public class ProductVariantUpdatedEventConsumer
{
    public static async Task HandleAsync(ProductVariantUpdatedEvent @event, ILogger<ProductVariantUpdatedEventConsumer> logger,
        AppDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[OrderingAPI] Consuming ProductVariantUpdatedEvent for variant {VariantId}",
            @event.Id);

        var now = DateTime.UtcNow;
        var cache = await dbContext.ProductCaches.FirstOrDefaultAsync(x => x.Id == @event.Id, cancellationToken);
        if (cache is null)
        {
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
        }
        else
        {
            cache.Sku = @event.Sku;
            cache.Name = @event.Name;
            cache.CurrentPrice = @event.CurrentPrice;
            cache.Currency = @event.Currency;
            cache.IsActive = @event.IsActive;
            cache.LastUpdatedUtc = now;
            cache.UpdatedAtUtc = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
