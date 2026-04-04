using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OrderingAPI.Domain.Entities;
using OrderingAPI.Messaging;
using Shared.Messaging.Catalog;
using OrderingAppDbContext = OrderingAPI.Domain.AppDbContext;

namespace DDDSample.Tests.Ordering;

public sealed class ProductCacheConsumerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<OrderingAppDbContext> _options;

    public ProductCacheConsumerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<OrderingAppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var ctx = NewContext();
        ctx.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private OrderingAppDbContext NewContext() => new(_options);

    [Fact]
    public async Task ProductUpdated_WhenCacheExists_UpdatesCachedFields()
    {
        var productId = Guid.NewGuid();
        await using (var seedContext = NewContext())
        {
            seedContext.ProductCaches.Add(new ProductCache
            {
                Id = productId,
                Sku = "product-slug",
                Name = "Existing Product",
                CurrentPrice = 100m,
                Currency = "USD",
                ImageUrl = "old.png",
                IsActive = true
            });
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        await ProductUpdatedEventConsumer.HandleAsync(
            new ProductUpdatedEvent
            {
                Id = productId,
                Name = "Updated Product",
                CurrentPrice = 125m,
                Currency = "EUR",
                Slug = "product-slug",
                ImageUrl = "new.png",
                IsActive = false
            },
            NullLogger<ProductUpdatedEventConsumer>.Instance,
            dbContext,
            CancellationToken.None);

        await using var assertContext = NewContext();
        var product = await assertContext.ProductCaches.SingleAsync(x => x.Id == productId);
        Assert.Equal("Updated Product", product.Name);
        Assert.Equal(125m, product.CurrentPrice);
        Assert.Equal("EUR", product.Currency);
        Assert.Equal("new.png", product.ImageUrl);
        Assert.False(product.IsActive);
    }

    [Fact]
    public async Task ProductDeleted_WhenCacheExists_RemovesProductCache()
    {
        var productId = Guid.NewGuid();
        await using (var seedContext = NewContext())
        {
            seedContext.ProductCaches.Add(new ProductCache
            {
                Id = productId,
                Sku = "product-slug",
                Name = "Existing Product",
                CurrentPrice = 100m,
                Currency = "USD"
            });
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        await ProductDeletedEventConsumer.HandleAsync(
            new ProductDeletedEvent { Id = productId },
            NullLogger<ProductDeletedEventConsumer>.Instance,
            dbContext,
            CancellationToken.None);

        await using var assertContext = NewContext();
        Assert.False(await assertContext.ProductCaches.AnyAsync(x => x.Id == productId));
    }
}

