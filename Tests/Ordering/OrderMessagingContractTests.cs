using Shared.Messaging.Order;
using Wolverine.Attributes;

namespace DDDSample.Tests.Ordering;

public sealed class OrderMessagingContractTests
{
    [Fact]
    public void OrderCreatedEvent_UsesCreatedRoutingKey()
    {
        // Arrange
        var attribute = typeof(OrderCreatedEvent).GetCustomAttributesData()
            .Single(x => x.AttributeType == typeof(TopicAttribute));

        // Assert
        Assert.Equal("ordering.order.created", attribute.ConstructorArguments.Single().Value);
    }
}
