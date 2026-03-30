using Microsoft.EntityFrameworkCore;
using PaymentAPI.Domain;
using PaymentAPI.Domain.Entities;
using PaymentAPI.Services.Grpc;
using Shared.Messaging.Order;
using Shared.ValueObjects;

namespace PaymentAPI.Features.Payments.Integration;

public class OrderCreatedEventHandler
{
    public static async Task HandleAsync(OrderCreatedEvent @event, AppDbContext dbContext,
        IOrderGrpcClientService orderGrpcClientService, ILogger<OrderCreatedEventHandler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("[PaymentAPI] Consuming OrderCreatedEvent for order {OrderId}", @event.Id);

        var exists = await dbContext.Payments.AnyAsync(x => x.OrderId == @event.Id, cancellationToken);
        if (exists)
        {
            logger.LogInformation("[PaymentAPI] Payment already exists for order {OrderId}; skipping duplicate OrderCreatedEvent",
                @event.Id);
            return;
        }

        var order = await orderGrpcClientService.GetOrderAsync(@event.Id, cancellationToken);
        var payment = Payment.CreatePending(order.OrderId, new Money(order.Total, order.Currency));

        await dbContext.Payments.AddAsync(payment, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[PaymentAPI] Created pending payment {PaymentId} for order {OrderId}", payment.Id, payment.OrderId);
    }
}
