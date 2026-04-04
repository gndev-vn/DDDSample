using Mediator;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PaymentAPI.Domain.Enums;
using PaymentAPI.Features.Messaging.OrderCanceled;
using PaymentAPI.Features.Messaging.OrderUpdated;
using PaymentAPI.Features.Payments.CreatePayment;
using Shared.Messaging.Order;
using Shared.ValueObjects;
using PaymentAppDbContext = PaymentAPI.Domain.AppDbContext;

namespace DDDSample.Tests.Payment;

public sealed class OrderLifecycleEventHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<PaymentAppDbContext> _options;

    public OrderLifecycleEventHandlerTests()
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
    public async Task OrderUpdated_WhenPendingPaymentExists_SynchronizesAmount()
    {
        var orderId = Guid.NewGuid();
        await using (var seedContext = NewContext())
        {
            seedContext.Payments.Add(PaymentAPI.Domain.Entities.Payment.CreatePending(orderId, new Money(50m, "USD")));
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var mediator = new Mock<IMediator>(MockBehavior.Strict);

        await OrderUpdatedEventHandler.HandleAsync(
            new OrderUpdatedEvent { Id = orderId, Total = 80m, Currency = "USD" },
            dbContext,
            mediator.Object,
            NullLogger<OrderUpdatedEventHandler>.Instance,
            CancellationToken.None);

        await using var assertContext = NewContext();
        var payment = await assertContext.Payments.SingleAsync(x => x.OrderId == orderId);
        Assert.Equal(80m, payment.Amount.Amount);
        Assert.Equal("USD", payment.Amount.Currency);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task OrderUpdated_WhenPaymentMissing_CreatesPendingPayment()
    {
        var orderId = Guid.NewGuid();
        await using var dbContext = NewContext();
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(x => x.Send(It.IsAny<CreatePaymentCommand>(), It.IsAny<CancellationToken>()))
            .Returns<CreatePaymentCommand, CancellationToken>((command, cancellationToken) =>
                new CreatePaymentCommandHandler(dbContext).Handle(command, cancellationToken));

        await OrderUpdatedEventHandler.HandleAsync(
            new OrderUpdatedEvent { Id = orderId, Total = 125m, Currency = "EUR" },
            dbContext,
            mediator.Object,
            NullLogger<OrderUpdatedEventHandler>.Instance,
            CancellationToken.None);

        await using var assertContext = NewContext();
        var payment = await assertContext.Payments.SingleAsync(x => x.OrderId == orderId);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(125m, payment.Amount.Amount);
        Assert.Equal("EUR", payment.Amount.Currency);
    }

    [Fact]
    public async Task OrderCanceled_WhenPendingPaymentExists_FailsPayment()
    {
        var orderId = Guid.NewGuid();
        await using (var seedContext = NewContext())
        {
            seedContext.Payments.Add(PaymentAPI.Domain.Entities.Payment.CreatePending(orderId, new Money(50m, "USD")));
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        await OrderCanceledEventHandler.HandleAsync(
            new OrderCanceledEvent { Id = orderId },
            dbContext,
            NullLogger<OrderCanceledEventHandler>.Instance,
            CancellationToken.None);

        await using var assertContext = NewContext();
        var payment = await assertContext.Payments.SingleAsync(x => x.OrderId == orderId);
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal("Order was cancelled before payment completion.", payment.FailureReason);
    }
}
