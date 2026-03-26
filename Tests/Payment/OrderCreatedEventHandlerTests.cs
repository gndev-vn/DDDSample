using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PaymentAPI.Domain.Entities;
using PaymentAPI.Domain.Enums;
using PaymentAPI.Features.Payments.Integration;
using PaymentAPI.Services.Grpc;
using Shared.Messaging.Order;
using PaymentAppDbContext = PaymentAPI.Domain.AppDbContext;

namespace DDDSample.Tests.Payment;

public sealed class OrderCreatedEventHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<PaymentAppDbContext> _options;

    public OrderCreatedEventHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<PaymentAppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var ctx = NewContext();
        ctx.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private PaymentAppDbContext NewContext() => new(_options);

    [Fact]
    public async Task HandleAsync_WhenPaymentDoesNotExist_CreatesPendingPaymentFromGrpcOrderSnapshot()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var grpc = new Mock<IOrderGrpcClientService>();
        grpc.Setup(x => x.GetOrderAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderSnapshot(orderId, Guid.NewGuid(), 125m, "USD", "Submitted"));

        await using var dbContext = NewContext();

        // Act
        await OrderCreatedEventHandler.HandleAsync(
            new OrderCreatedEvent { Id = orderId },
            dbContext,
            grpc.Object,
            NullLogger<OrderCreatedEventHandler>.Instance,
            CancellationToken.None);

        // Assert
        await using var assertContext = NewContext();
        var payment = await assertContext.Payments.SingleAsync(x => x.OrderId == orderId);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(125m, payment.Amount.Amount);
        Assert.Equal("USD", payment.Amount.Currency);
        grpc.Verify(x => x.GetOrderAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenPaymentAlreadyExists_SkipsGrpcLookupAndDoesNotDuplicate()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        await using (var seedContext = NewContext())
        {
            seedContext.Payments.Add(PaymentAPI.Domain.Entities.Payment.CreatePending(orderId, new Shared.ValueObjects.Money(30m, "USD")));
            await seedContext.SaveChangesAsync();
        }

        var grpc = new Mock<IOrderGrpcClientService>(MockBehavior.Strict);
        await using var dbContext = NewContext();

        // Act
        await OrderCreatedEventHandler.HandleAsync(
            new OrderCreatedEvent { Id = orderId },
            dbContext,
            grpc.Object,
            NullLogger<OrderCreatedEventHandler>.Instance,
            CancellationToken.None);

        // Assert
        await using var assertContext = NewContext();
        Assert.Equal(1, await assertContext.Payments.CountAsync(x => x.OrderId == orderId));
        grpc.VerifyNoOtherCalls();
    }
}
