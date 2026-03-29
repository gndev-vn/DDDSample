using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OrderingAPI.Domain.Entities;
using OrderingAPI.Messaging;
using Shared.Messaging.Catalog;
using OrderingAppDbContext = OrderingAPI.Domain.AppDbContext;

namespace DDDSample.Tests.Ordering;

public sealed class CategoryCacheConsumerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<OrderingAppDbContext> _options;

    public CategoryCacheConsumerTests()
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
    public async Task CategoryUpdated_WhenCacheExists_UpdatesCachedFields()
    {
        var categoryId = Guid.NewGuid();
        await using (var seedContext = NewContext())
        {
            seedContext.CategoryCaches.Add(new CategoryCache
            {
                Id = categoryId,
                Name = "Existing Category",
                ParentId = null,
                IsActive = true
            });
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        await CategoryUpdatedEventConsumer.HandleAsync(
            new CategoryUpdatedEvent
            {
                Id = categoryId,
                Name = "Updated Category",
                ParentId = Guid.NewGuid(),
                IsActive = false
            },
            NullLogger<CategoryUpdatedEventConsumer>.Instance,
            dbContext,
            CancellationToken.None);

        await using var assertContext = NewContext();
        var category = await assertContext.CategoryCaches.SingleAsync(x => x.Id == categoryId);
        Assert.Equal("Updated Category", category.Name);
        Assert.False(category.IsActive);
        Assert.NotNull(category.ParentId);
    }

    [Fact]
    public async Task CategoryDeleted_WhenCacheExists_RemovesCategoryCache()
    {
        var categoryId = Guid.NewGuid();
        await using (var seedContext = NewContext())
        {
            seedContext.CategoryCaches.Add(new CategoryCache
            {
                Id = categoryId,
                Name = "Existing Category"
            });
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        await CategoryDeletedEventConsumer.HandleAsync(
            new CategoryDeletedEvent { Id = categoryId },
            NullLogger<CategoryDeletedEventConsumer>.Instance,
            dbContext,
            CancellationToken.None);

        await using var assertContext = NewContext();
        Assert.False(await assertContext.CategoryCaches.AnyAsync(x => x.Id == categoryId));
    }
}
