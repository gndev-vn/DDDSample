using Shared.Messaging;
using Shared.Messaging.Order;
using Wolverine.Attributes;

namespace DDDSample.Tests.Ordering;

public sealed class OrderMessagingContractTests
{
    [Theory]
    [InlineData(typeof(OrderCreatedEvent), KafkaTopics.Ordering.OrderCreated)]
    [InlineData(typeof(OrderUpdatedEvent), KafkaTopics.Ordering.OrderUpdated)]
    [InlineData(typeof(OrderCanceledEvent), KafkaTopics.Ordering.OrderCanceled)]
    [InlineData(typeof(OrderPaidEvent), KafkaTopics.Ordering.OrderPaid)]
    [InlineData(typeof(OrderLineAddedEvent), KafkaTopics.Ordering.OrderLineAdded)]
    public void OrderIntegrationEvents_UseExpectedKafkaTopics(Type eventType, string expectedTopic)
    {
        var attribute = eventType.GetCustomAttributesData()
            .Single(x => x.AttributeType == typeof(TopicAttribute));

        Assert.Equal(expectedTopic, attribute.ConstructorArguments.Single().Value);
    }
}
