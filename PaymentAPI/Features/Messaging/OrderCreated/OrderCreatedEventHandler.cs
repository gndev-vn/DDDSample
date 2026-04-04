using Mediator;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.Domain;
using PaymentAPI.Features.Payments.CreatePayment;
using Shared.Messaging.Order;
using Wolverine.Attributes;

namespace PaymentAPI.Features.Messaging.OrderCreated;

[ScheduleRetry(typeof(DbUpdateException), 5, 15, 30)]
[RetryNow(typeof(TimeoutException), 50, 100, 250)]
public class OrderCreatedEventHandler
{
    public static async Task HandleAsync(
        OrderCreatedEvent @event,
        AppDbContext dbContext,
        IMediator mediator,
        ILogger<OrderCreatedEventHandler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("[PaymentAPI] Consuming OrderCreatedEvent for order {OrderId}", @event.Id);

        var exists = await dbContext.Payments
            .AsNoTracking()
            .AnyAsync(x => x.OrderId == @event.Id, cancellationToken);
        if (exists)
        {
            logger.LogInformation("[PaymentAPI] Payment already exists for order {OrderId}; skipping duplicate OrderCreatedEvent", @event.Id);
            return;
        }

        var payment = await mediator.Send(new CreatePaymentCommand(@event.Id, @event.Total, @event.Currency), cancellationToken);

        logger.LogInformation("[PaymentAPI] Created pending payment {PaymentId} for order {OrderId}", payment.Id, payment.OrderId);
    }
}

