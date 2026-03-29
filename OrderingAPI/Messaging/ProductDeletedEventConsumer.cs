using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using Shared.Messaging.Catalog;

namespace OrderingAPI.Messaging;

public class ProductDeletedEventConsumer
{
    public static async Task HandleAsync(ProductDeletedEvent @event, ILogger<ProductDeletedEventConsumer> logger,
        AppDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("Consumed product deleted event {Id}", @event.Id);
        var cache = await dbContext.ProductCaches.FirstOrDefaultAsync(x => x.Id == @event.Id, cancellationToken);
        if (cache is null)
        {
            return;
        }

        dbContext.ProductCaches.Remove(cache);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
