using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OrderingAPI.Domain.Entities;
using OrderingAPI.Messaging;
using Shared.Messaging.Catalog;
using OrderingAppDbContext = OrderingAPI.Domain.AppDbContext;

namespace DDDSample.Tests.Ordering;

public sealed class ProductCreatedEventConsumerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<OrderingAppDbContext> _options;

    public ProductCreatedEventConsumerTests()
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
    public async Task HandleAsync_WhenProductDoesNotExist_AddsProductCache()
    {
        // Arrange
        var productId = Guid.NewGuid();
        await using var dbContext = NewContext();

        // Act
        await ProductCreatedEventConsumer.HandleAsync(
            new ProductCreatedEvent
            {
                Id = productId,
                Sku = "product-slug",
                Name = "Sample Product",
                CurrentPrice = 105m,
                Currency = "USD",
                Slug = "product-slug",
                ImageUrl = "image.png"
            },
            NullLogger<ProductCreatedEventConsumer>.Instance,
            dbContext,
            CancellationToken.None);

        // Assert
        await using var assertContext = NewContext();
        var product = await assertContext.ProductCaches.SingleAsync(x => x.Id == productId);
        Assert.Equal("product-slug", product.Sku);
        Assert.Equal("Sample Product", product.Name);
        Assert.Equal(105m, product.CurrentPrice);
        Assert.Equal("USD", product.Currency);
    }

    [Fact]
    public async Task HandleAsync_WhenDuplicateProductArrives_IgnoresEvent()
    {
        // Arrange
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

        // Act
        await ProductCreatedEventConsumer.HandleAsync(
            new ProductCreatedEvent
            {
                Id = productId,
                Sku = "product-slug",
                Name = "Sample Product",
                CurrentPrice = 105m,
                Currency = "USD",
                Slug = "product-slug",
                ImageUrl = "image.png"
            },
            NullLogger<ProductCreatedEventConsumer>.Instance,
            dbContext,
            CancellationToken.None);

        // Assert
        await using var assertContext = NewContext();
        Assert.Equal(1, await assertContext.ProductCaches.CountAsync(x => x.Sku == "product-slug"));
        var existing = await assertContext.ProductCaches.SingleAsync(x => x.Id == productId);
        Assert.Equal("Existing Product", existing.Name);
        Assert.Equal(100m, existing.CurrentPrice);
    }
}
