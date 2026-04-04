using System.Reflection;
using OrderingAPI.Features.Orders.Integration;
using OrderingAPI.Messaging;
using PaymentAPI.Features.Messaging.OrderCanceled;
using PaymentAPI.Features.Messaging.OrderCreated;
using PaymentAPI.Features.Messaging.OrderUpdated;
using Wolverine.Attributes;

namespace DDDSample.Tests.Configuration;

public sealed class KafkaRetryPolicyTests
{
    [Theory]
    [InlineData(typeof(ProductCreatedEventConsumer))]
    [InlineData(typeof(ProductUpdatedEventConsumer))]
    [InlineData(typeof(ProductDeletedEventConsumer))]
    [InlineData(typeof(ProductVariantCreatedEventConsumer))]
    [InlineData(typeof(ProductVariantUpdatedEventConsumer))]
    [InlineData(typeof(ProductVariantDeletedEventConsumer))]
    [InlineData(typeof(PaymentCreatedEventHandler))]
    [InlineData(typeof(PaymentCompletedEventHandler))]
    [InlineData(typeof(PaymentFailedEventHandler))]
    [InlineData(typeof(OrderCreatedEventHandler))]
    [InlineData(typeof(OrderUpdatedEventHandler))]
    [InlineData(typeof(OrderCanceledEventHandler))]
    public void KafkaConsumers_UseExpectedRetryPolicies(Type handlerType)
    {
        var scheduleRetry = handlerType.GetCustomAttribute<ScheduleRetryAttribute>();
        Assert.NotNull(scheduleRetry);

        var retryNow = handlerType.GetCustomAttribute<RetryNowAttribute>();
        Assert.NotNull(retryNow);
    }
}
