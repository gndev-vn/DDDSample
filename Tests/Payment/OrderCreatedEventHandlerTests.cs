using Mediator;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PaymentAPI.Domain.Enums;
using PaymentAPI.Features.Messaging.OrderCreated;
using PaymentAPI.Features.Payments.CreatePayment;
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
    public async Task HandleAsync_WhenPaymentDoesNotExist_CreatesPendingPaymentFromOrderCreatedEvent()
    {
        var orderId = Guid.NewGuid();
        await using var dbContext = NewContext();
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(x => x.Send(It.IsAny<CreatePaymentCommand>(), It.IsAny<CancellationToken>()))
            .Returns<CreatePaymentCommand, CancellationToken>((command, cancellationToken) =>
                new CreatePaymentCommandHandler(dbContext).Handle(command, cancellationToken));

        await OrderCreatedEventHandler.HandleAsync(
            new OrderCreatedEvent { Id = orderId, Total = 125m, Currency = "USD" },
            dbContext,
            mediator.Object,
            NullLogger<OrderCreatedEventHandler>.Instance,
            CancellationToken.None);

        await using var assertContext = NewContext();
        var payment = await assertContext.Payments.SingleAsync(x => x.OrderId == orderId);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(125m, payment.Amount.Amount);
        Assert.Equal("USD", payment.Amount.Currency);
        mediator.Verify(x => x.Send(
                It.Is<CreatePaymentCommand>(command => command.OrderId == orderId && command.Amount == 125m && command.Currency == "USD"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenPaymentAlreadyExists_DoesNotDuplicate()
    {
        var orderId = Guid.NewGuid();
        await using (var seedContext = NewContext())
        {
            seedContext.Payments.Add(PaymentAPI.Domain.Entities.Payment.CreatePending(orderId, new Shared.ValueObjects.Money(30m, "USD")));
            await seedContext.SaveChangesAsync();
        }

        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        await using var dbContext = NewContext();

        await OrderCreatedEventHandler.HandleAsync(
            new OrderCreatedEvent { Id = orderId, Total = 30m, Currency = "USD" },
            dbContext,
            mediator.Object,
            NullLogger<OrderCreatedEventHandler>.Instance,
            CancellationToken.None);

        await using var assertContext = NewContext();
        Assert.Equal(1, await assertContext.Payments.CountAsync(x => x.OrderId == orderId));
        mediator.VerifyNoOtherCalls();
    }
}
