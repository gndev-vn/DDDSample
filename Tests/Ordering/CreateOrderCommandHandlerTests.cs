using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using OrderingAPI.Features.Orders.CreateOrder;
using OrderingAPI.Features.Orders.GetOrderById;
using Shared.Models;

namespace DDDSample.Tests.Ordering;

public sealed class CreateOrderCommandHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public CreateOrderCommandHandlerTests()
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
    public async Task Handle_WhenProductExistsInLocalCache_UsesCachedPriceAndProductId()
    {
        var productId = Guid.NewGuid();
        await using (var seedContext = NewContext())
        {
            seedContext.ProductCaches.Add(new ProductCache
            {
                Id = productId,
                Sku = "SKU-1",
                Name = "Cached Product",
                CurrentPrice = 25m,
                Currency = "USD",
                IsActive = true
            });
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new CreateOrderCommandHandler(dbContext);
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            [new OrderLineModel { Sku = "sku-1", Quantity = 2, UnitPrice = 5m, Currency = "VND" }],
            new AddressModel("123 Main", null, "City", "Province", "District", "Ward"),
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        await using var assertContext = NewContext();
        var order = await assertContext.Orders.Include(x => x.Lines).SingleAsync(x => x.Id == result.Id);
        var line = Assert.Single(order.Lines);
        Assert.Equal(productId, line.ProductId);
        Assert.Equal(25m, line.Total.Amount);
        Assert.Equal("USD", line.Total.Currency);
        Assert.Equal("Cached Product", result.Lines.Single().Name);
    }
}