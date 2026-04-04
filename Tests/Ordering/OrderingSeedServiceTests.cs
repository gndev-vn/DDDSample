using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrderingAPI.Configuration;
using OrderingAPI.Domain;
using OrderingAPI.Services;
using Shared.Enums;

namespace DDDSample.Tests.Ordering;

public sealed class OrderingSeedServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public OrderingSeedServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var ctx = NewContext();
        ctx.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private AppDbContext NewContext() => new(_options);

    [Fact]
    public async Task SeedAsync_CreatesOrderingDemoData_WithoutDuplicates()
    {
        await using var seedContext = NewContext();
        var service = new OrderingSeedService(
            seedContext,
            Options.Create(new OrderingSeedOptions { Enabled = true }),
            NullLogger<OrderingSeedService>.Instance);

        await service.SeedAsync();
        await service.SeedAsync();

        await using var assertContext = NewContext();
        var customers = await assertContext.Customers.ToListAsync();
        var productCaches = await assertContext.ProductCaches.ToListAsync();
        var orders = await assertContext.Orders.Include(order => order.Lines).ToListAsync();

        Assert.Equal(2, customers.Count);
        Assert.Equal(3, productCaches.Count);
        Assert.Equal(2, orders.Count);
        Assert.Contains(customers, customer => customer.Email == "alex.nguyen@example.com");
        Assert.Contains(orders, order => order.Status == OrderStatus.Submitted);
        Assert.Contains(orders, order => order.Status == OrderStatus.Paid);
        Assert.All(orders, order =>
        {
          Assert.NotEmpty(order.CustomerName);
          Assert.NotEmpty(order.CustomerEmail);
          Assert.NotEmpty(order.Lines);
        });
    }
}
