using Shared.Messaging;
using Shared.Messaging.Catalog;
using Shared.Messaging.Order;
using Shared.Messaging.Payment;
using Wolverine.Attributes;

namespace DDDSample.Tests.Configuration;

public sealed class KafkaTopicContractTests
{
    [Theory]
    [InlineData(typeof(CategoryCreatedEvent), KafkaTopics.Catalog.CategoryCreated)]
    [InlineData(typeof(CategoryUpdatedEvent), KafkaTopics.Catalog.CategoryUpdated)]
    [InlineData(typeof(CategoryDeletedEvent), KafkaTopics.Catalog.CategoryDeleted)]
    [InlineData(typeof(ProductCreatedEvent), KafkaTopics.Catalog.ProductCreated)]
    [InlineData(typeof(ProductUpdatedEvent), KafkaTopics.Catalog.ProductUpdated)]
    [InlineData(typeof(ProductDeletedEvent), KafkaTopics.Catalog.ProductDeleted)]
    [InlineData(typeof(ProductVariantCreatedEvent), KafkaTopics.Catalog.ProductVariantCreated)]
    [InlineData(typeof(ProductVariantUpdatedEvent), KafkaTopics.Catalog.ProductVariantUpdated)]
    [InlineData(typeof(ProductVariantDeletedEvent), KafkaTopics.Catalog.ProductVariantDeleted)]
    [InlineData(typeof(OrderCreatedEvent), KafkaTopics.Ordering.OrderCreated)]
    [InlineData(typeof(OrderUpdatedEvent), KafkaTopics.Ordering.OrderUpdated)]
    [InlineData(typeof(OrderCanceledEvent), KafkaTopics.Ordering.OrderCanceled)]
    [InlineData(typeof(OrderPaidEvent), KafkaTopics.Ordering.OrderPaid)]
    [InlineData(typeof(OrderLineAddedEvent), KafkaTopics.Ordering.OrderLineAdded)]
    [InlineData(typeof(PaymentCreatedEvent), KafkaTopics.Payment.PaymentCreated)]
    [InlineData(typeof(PaymentCompletedEvent), KafkaTopics.Payment.PaymentCompleted)]
    [InlineData(typeof(PaymentFailedEvent), KafkaTopics.Payment.PaymentFailed)]
    public void IntegrationContracts_UseExpectedKafkaTopics(Type eventType, string expectedTopic)
    {
        var attribute = eventType.GetCustomAttributesData()
            .Single(x => x.AttributeType == typeof(TopicAttribute));

        Assert.Equal(expectedTopic, attribute.ConstructorArguments.Single().Value);
    }
}
