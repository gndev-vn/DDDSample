using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.Domain.Entities;
using PaymentAPI.Domain.Enums;
using PaymentAPI.Features.Payments.CompletePayment;
using PaymentAPI.Features.Payments.FailPayment;
using Shared.Exceptions;
using Shared.ValueObjects;
using PaymentAppDbContext = PaymentAPI.Domain.AppDbContext;

namespace DDDSample.Tests.Payment;

public sealed class PaymentCommandHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<PaymentAppDbContext> _options;

    public PaymentCommandHandlerTests()
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

    private async Task<Guid> SeedPendingPaymentAsync(decimal amount = 50m, string currency = "USD")
    {
        var payment = PaymentAPI.Domain.Entities.Payment.CreatePending(Guid.NewGuid(), new Money(amount, currency));

        await using var ctx = NewContext();
        ctx.Payments.Add(payment);
        await ctx.SaveChangesAsync();
        return payment.Id;
    }

    [Fact]
    public async Task CompletePayment_WhenPaymentIsPending_CompletesAndReturnsUpdatedModel()
    {
        var paymentId = await SeedPendingPaymentAsync();
        await using var handlerContext = NewContext();
        var handler = new CompletePaymentCommandHandler(handlerContext);

        var result = await handler.Handle(new CompletePaymentCommand(paymentId, "txn-123"), CancellationToken.None);

        await using var assertContext = NewContext();
        var payment = await assertContext.Payments.SingleAsync(x => x.Id == paymentId);
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Equal("txn-123", payment.TransactionReference);
        Assert.NotNull(payment.CompletedAtUtc);
        Assert.Equal(PaymentStatus.Completed, result.Status);
        Assert.Equal("txn-123", result.TransactionReference);
    }

    [Fact]
    public async Task FailPayment_WhenPaymentIsPending_MarksPaymentAsFailed()
    {
        var paymentId = await SeedPendingPaymentAsync();
        await using var handlerContext = NewContext();
        var handler = new FailPaymentCommandHandler(handlerContext);

        var result = await handler.Handle(new FailPaymentCommand(paymentId, "Gateway timeout"), CancellationToken.None);

        await using var assertContext = NewContext();
        var payment = await assertContext.Payments.SingleAsync(x => x.Id == paymentId);
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal("Gateway timeout", payment.FailureReason);
        Assert.Null(payment.TransactionReference);
        Assert.Null(payment.CompletedAtUtc);
        Assert.Equal(PaymentStatus.Failed, result.Status);
        Assert.Equal("Gateway timeout", result.FailureReason);
    }

    [Fact]
    public async Task CompletePayment_WhenPaymentDoesNotExist_ThrowsNotFoundException()
    {
        await using var handlerContext = NewContext();
        var handler = new CompletePaymentCommandHandler(handlerContext);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CompletePaymentCommand(Guid.NewGuid(), "txn-404"), CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task CompletePayment_WhenPaymentAlreadyFailed_ThrowsDomainException()
    {
        var paymentId = await SeedPendingPaymentAsync();
        await using (var arrangeContext = NewContext())
        {
            var payment = await arrangeContext.Payments.SingleAsync(x => x.Id == paymentId);
            payment.Fail("Gateway timeout");
            await arrangeContext.SaveChangesAsync();
        }

        await using var handlerContext = NewContext();
        var handler = new CompletePaymentCommandHandler(handlerContext);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(new CompletePaymentCommand(paymentId, "txn-late"), CancellationToken.None).AsTask());
        Assert.Contains("pending", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CompletePaymentRequestValidator_WhenRequestIsInvalid_ReturnsExpectedErrors()
    {
        var validator = new CompletePaymentRequestValidator();
        var request = new CompletePaymentRequest { TransactionReference = string.Empty };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CompletePaymentRequest.TransactionReference));
    }

    [Fact]
    public void FailPaymentRequestValidator_WhenRequestIsInvalid_ReturnsExpectedErrors()
    {
        var validator = new FailPaymentRequestValidator();
        var request = new FailPaymentRequest { Reason = string.Empty };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(FailPaymentRequest.Reason));
    }
}