using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using Shared.Messaging.Catalog;

namespace OrderingAPI.Messaging;

public class ProductUpdatedEventConsumer
{
    public static async Task HandleAsync(ProductUpdatedEvent @event, ILogger<ProductUpdatedEventConsumer> logger,
        AppDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("[OrderingAPI] Consuming ProductUpdatedEvent for product {ProductId}", @event.Id);
        var now = DateTime.UtcNow;
        var cache = await dbContext.ProductCaches.FirstOrDefaultAsync(x => x.Id == @event.Id, cancellationToken);
        if (cache is null)
        {
            await dbContext.ProductCaches.AddAsync(new ProductCache
            {
                Id = @event.Id,
                Sku = @event.Slug,
                Name = @event.Name,
                CurrentPrice = @event.CurrentPrice,
                Currency = @event.Currency,
                ImageUrl = @event.ImageUrl,
                IsActive = @event.IsActive,
                LastUpdatedUtc = now,
                UpdatedAtUtc = now
            }, cancellationToken);
        }
        else
        {
            cache.Name = @event.Name;
            cache.CurrentPrice = @event.CurrentPrice;
            cache.Currency = @event.Currency;
            cache.ImageUrl = @event.ImageUrl;
            cache.IsActive = @event.IsActive;
            cache.LastUpdatedUtc = now;
            cache.UpdatedAtUtc = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
