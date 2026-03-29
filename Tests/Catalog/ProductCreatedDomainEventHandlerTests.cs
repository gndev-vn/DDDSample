using CatalogAPI.Domain.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shared.Messaging.Catalog;
using Wolverine;

namespace DDDSample.Tests.Catalog;

public sealed class ProductCreatedDomainEventHandlerTests
{
    [Fact]
    public async Task Handle_PublishesProductCreatedEventWithSkuAndSlug()
    {
        ProductCreatedEvent? published = null;
        var bus = new Mock<IMessageBus>();
        bus.Setup(x => x.PublishAsync(It.IsAny<ProductCreatedEvent>(), It.IsAny<DeliveryOptions>()))
            .Callback<ProductCreatedEvent, DeliveryOptions?>((message, _) => published = message)
            .Returns(ValueTask.CompletedTask);

        var domainEvent = new ProductCreatedDomainEvent
        {
            Id = Guid.NewGuid(),
            Sku = "product-slug",
            Name = "Sample Product",
            CurrentPrice = 105m,
            Currency = "USD",
            Slug = "product-slug",
            ImageUrl = "image.png",
            IsActive = true
        };

        await ProductCreatedDomainEventHandler.Handle(domainEvent, bus.Object,
            NullLogger<ProductCreatedDomainEventHandler>.Instance);

        Assert.NotNull(published);
        Assert.Equal(domainEvent.Id, published!.Id);
        Assert.Equal("product-slug", published.Sku);
        Assert.Equal(domainEvent.Slug, published.Slug);
        Assert.Equal(105m, published.CurrentPrice);
        Assert.Equal("USD", published.Currency);
        Assert.True(published.IsActive);
    }
}
