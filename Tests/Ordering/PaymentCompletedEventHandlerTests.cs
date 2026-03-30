using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OrderingAPI.Domain.Entities;
using OrderingAPI.Features.Orders.Integration;
using Shared.Enums;
using Shared.Messaging.Payment;
using Shared.ValueObjects;
using OrderingAppDbContext = OrderingAPI.Domain.AppDbContext;

namespace DDDSample.Tests.Ordering;

public sealed class PaymentCompletedEventHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<OrderingAppDbContext> _options;

    public PaymentCompletedEventHandlerTests()
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

    private async Task<Guid> SeedOrderAsync(OrderStatus status = OrderStatus.Submitted)
    {
        var order = Order.Create(
            Guid.NewGuid(),
            [new OrderLine(new Sku("SKU-001"), Quantity.Of(1), new Money(40m, "USD"))],
            new Address("123 Main", null, "Ward 1", "District 1", "HCMC", "HCM"));

        if (status == OrderStatus.Paid)
        {
            order.Pay();
        }

        await using var ctx = NewContext();
        ctx.Orders.Add(order);
        await ctx.SaveChangesAsync();
        return order.Id;
    }

    [Fact]
    public async Task HandleAsync_WhenOrderExists_MarksOrderAsPaid()
    {
        // Arrange
        var orderId = await SeedOrderAsync();
        await using var dbContext = NewContext();

        // Act
        await PaymentCompletedEventHandler.HandleAsync(
            new PaymentCompletedEvent
            {
                PaymentId = Guid.NewGuid(),
                OrderId = orderId,
                Amount = 40m,
                Currency = "USD",
                TransactionReference = "txn-123"
            },
            dbContext,
            NullLogger<PaymentCompletedEventHandler>.Instance,
            CancellationToken.None);

        // Assert
        await using var assertContext = NewContext();
        var order = await assertContext.Orders.SingleAsync(x => x.Id == orderId);
        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Fact]
    public async Task HandleAsync_WhenOrderAlreadyPaid_IgnoresDuplicateEvent()
    {
        // Arrange
        var orderId = await SeedOrderAsync(OrderStatus.Paid);
        await using var dbContext = NewContext();

        // Act
        await PaymentCompletedEventHandler.HandleAsync(
            new PaymentCompletedEvent
            {
                PaymentId = Guid.NewGuid(),
                OrderId = orderId,
                Amount = 40m,
                Currency = "USD",
                TransactionReference = "txn-duplicate"
            },
            dbContext,
            NullLogger<PaymentCompletedEventHandler>.Instance,
            CancellationToken.None);

        // Assert
        await using var assertContext = NewContext();
        var order = await assertContext.Orders.SingleAsync(x => x.Id == orderId);
        Assert.Equal(OrderStatus.Paid, order.Status);
    }
}
