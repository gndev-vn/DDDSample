using Mediator;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.Domain;
using PaymentAPI.Domain.Entities;
using Shared.Messaging.Order;
using Shared.ValueObjects;
using Wolverine.Attributes;

namespace PaymentAPI.Features.Messaging.OrderUpdated;

[ScheduleRetry(typeof(DbUpdateException), 5, 15, 30)]
[RetryNow(typeof(TimeoutException), 50, 100, 250)]
public class OrderUpdatedEventHandler
{
    public static async Task HandleAsync(OrderUpdatedEvent @event, AppDbContext dbContext,
        IMediator mediator, ILogger<OrderUpdatedEventHandler> logger, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[PaymentAPI] Consuming OrderUpdatedEvent for order {OrderId} with total {Total} {Currency}",
            @event.Id,
            @event.Total,
            @event.Currency);

        var payment = await dbContext.Payments.FirstOrDefaultAsync(x => x.OrderId == @event.Id, cancellationToken);
        if (payment is null)
        {
            logger.LogInformation(
                "[PaymentAPI] No payment exists for order {OrderId}; creating pending payment from OrderUpdatedEvent",
                @event.Id);

            await mediator.Send(new PaymentAPI.Features.Payments.CreatePayment.CreatePaymentCommand(@event.Id, @event.Total, @event.Currency), cancellationToken);
            return;
        }

        if (payment.Status != Domain.Enums.PaymentStatus.Pending)
        {
            logger.LogInformation(
                "[PaymentAPI] Payment {PaymentId} for order {OrderId} is {Status}; skipping amount synchronization",
                payment.Id,
                @event.Id,
                payment.Status);
            return;
        }

        payment.SynchronizePendingAmount(new Money(@event.Total, @event.Currency));
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "[PaymentAPI] Synchronized pending payment {PaymentId} for order {OrderId} to {Total} {Currency}",
            payment.Id,
            @event.Id,
            @event.Total,
            @event.Currency);
    }
}

