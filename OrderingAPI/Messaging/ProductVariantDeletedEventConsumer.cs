using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;
using OrderingAPI.Domain;
using Shared.Messaging.Catalog;

namespace OrderingAPI.Messaging;

[ScheduleRetry(typeof(DbUpdateException), 5, 15, 30)]
[RetryNow(typeof(TimeoutException), 50, 100, 250)]
public class ProductVariantDeletedEventConsumer
{
    public static async Task HandleAsync(ProductVariantDeletedEvent @event, ILogger<ProductVariantDeletedEventConsumer> logger,
        AppDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[OrderingAPI] Consuming ProductVariantDeletedEvent for variant {VariantId}",
            @event.Id);

        var cache = await dbContext.ProductCaches.FirstOrDefaultAsync(x => x.Id == @event.Id, cancellationToken);
        if (cache is null)
        {
            logger.LogInformation(
                "[OrderingAPI] Product cache does not exist for variant {VariantId}; skipping ProductVariantDeletedEvent",
                @event.Id);
            return;
        }

        dbContext.ProductCaches.Remove(cache);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
