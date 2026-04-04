using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OrderingAPI.Domain.Entities;
using OrderingAPI.Messaging;
using Shared.Messaging.Catalog;
using OrderingAppDbContext = OrderingAPI.Domain.AppDbContext;

namespace DDDSample.Tests.Ordering;

public sealed class ProductVariantConsumerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<OrderingAppDbContext> _options;

    public ProductVariantConsumerTests()
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
    public async Task VariantCreated_AddsProductCache()
    {
        var variantId = Guid.NewGuid();
        await using var dbContext = NewContext();

        await ProductVariantCreatedEventConsumer.HandleAsync(
            new ProductVariantCreatedEvent
            {
                Id = variantId,
                ProductId = Guid.NewGuid(),
                Sku = "TSHIRT-BLACK-M",
                Name = "T-Shirt Black / M",
                CurrentPrice = 199m,
                Currency = "USD",
                IsActive = true
            },
            NullLogger<ProductVariantCreatedEventConsumer>.Instance,
            dbContext,
            CancellationToken.None);

        await using var assertContext = NewContext();
        var product = await assertContext.ProductCaches.SingleAsync(x => x.Id == variantId);
        Assert.Equal("TSHIRT-BLACK-M", product.Sku);
        Assert.Equal("T-Shirt Black / M", product.Name);
        Assert.Equal(199m, product.CurrentPrice);
    }

    [Fact]
    public async Task VariantUpdated_UpdatesCachedFields()
    {
        var variantId = Guid.NewGuid();
        await using (var seedContext = NewContext())
        {
            seedContext.ProductCaches.Add(new ProductCache
            {
                Id = variantId,
                Sku = "TSHIRT-BLACK-M",
                Name = "Old Variant",
                CurrentPrice = 150m,
                Currency = "USD",
                IsActive = true
            });
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        await ProductVariantUpdatedEventConsumer.HandleAsync(
            new ProductVariantUpdatedEvent
            {
                Id = variantId,
                ProductId = Guid.NewGuid(),
                Sku = "TSHIRT-BLACK-L",
                Name = "Updated Variant",
                CurrentPrice = 205m,
                Currency = "EUR",
                IsActive = false
            },
            NullLogger<ProductVariantUpdatedEventConsumer>.Instance,
            dbContext,
            CancellationToken.None);

        await using var assertContext = NewContext();
        var product = await assertContext.ProductCaches.SingleAsync(x => x.Id == variantId);
        Assert.Equal("TSHIRT-BLACK-L", product.Sku);
        Assert.Equal("Updated Variant", product.Name);
        Assert.Equal(205m, product.CurrentPrice);
        Assert.Equal("EUR", product.Currency);
        Assert.False(product.IsActive);
    }

    [Fact]
    public async Task VariantDeleted_RemovesProductCache()
    {
        var variantId = Guid.NewGuid();
        await using (var seedContext = NewContext())
        {
            seedContext.ProductCaches.Add(new ProductCache
            {
                Id = variantId,
                Sku = "TSHIRT-BLACK-M",
                Name = "Existing Variant",
                CurrentPrice = 150m,
                Currency = "USD"
            });
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        await ProductVariantDeletedEventConsumer.HandleAsync(
            new ProductVariantDeletedEvent { Id = variantId, ProductId = Guid.NewGuid(), Sku = "TSHIRT-BLACK-M" },
            NullLogger<ProductVariantDeletedEventConsumer>.Instance,
            dbContext,
            CancellationToken.None);

        await using var assertContext = NewContext();
        Assert.False(await assertContext.ProductCaches.AnyAsync(x => x.Id == variantId));
    }
}
