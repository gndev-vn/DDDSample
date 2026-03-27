using Mapster;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Database;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using Shared.Messaging.Catalog;

namespace OrderingAPI.Messaging;

public class ProductCreatedEventConsumer
{
    public static async Task HandleAsync(ProductCreatedEvent @event, ILogger<ProductCreatedEventConsumer> logger,
        AppDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("Consumed product created event {Id}", @event.Id);
        var product = @event.Adapt<ProductCache>();
        if (await dbContext.ProductCaches.AnyAsync(x => x.Id == product.Id || x.Sku == product.Sku, cancellationToken))
        {
            logger.LogInformation("Product already exists {Id}", @event.Id);
            return;
        }

        await dbContext.ProductCaches.AddAsync(product, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}