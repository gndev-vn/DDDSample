using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using Shared.Messaging.Payment;
using Wolverine.Attributes;

namespace OrderingAPI.Features.Orders.Integration;

[ScheduleRetry(typeof(DbUpdateException), 5, 15, 30)]
[RetryNow(typeof(TimeoutException), 50, 100, 250)]
public class PaymentCreatedEventHandler
{
    public static async Task HandleAsync(PaymentCreatedEvent @event, AppDbContext dbContext,
        ILogger<PaymentCreatedEventHandler> logger, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[OrderingAPI] Consuming PaymentCreatedEvent for order {OrderId} and payment {PaymentId}",
            @event.OrderId,
            @event.PaymentId);

        var orderExists = await dbContext.Orders
            .AsNoTracking()
            .AnyAsync(x => x.Id == @event.OrderId, cancellationToken);

        if (!orderExists)
        {
            logger.LogWarning(
                "[OrderingAPI] Order {OrderId} was not found for PaymentCreatedEvent {PaymentId}",
                @event.OrderId,
                @event.PaymentId);
            return;
        }

        logger.LogInformation(
            "[OrderingAPI] Payment {PaymentId} was created for order {OrderId} with amount {Amount} {Currency}",
            @event.PaymentId,
            @event.OrderId,
            @event.Amount,
            @event.Currency);
    }
}
