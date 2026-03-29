using CatalogAPI.Domain.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shared.Messaging.Catalog;
using Wolverine;

namespace DDDSample.Tests.Catalog;

public sealed class ProductVariantDomainEventHandlerTests
{
    [Fact]
    public async Task HandleCreated_PublishesProductVariantCreatedEvent()
    {
        ProductVariantCreatedEvent? published = null;
        var bus = new Mock<IMessageBus>();
        bus.Setup(x => x.PublishAsync(It.IsAny<ProductVariantCreatedEvent>(), It.IsAny<DeliveryOptions>()))
            .Callback<ProductVariantCreatedEvent, DeliveryOptions?>((message, _) => published = message)
            .Returns(ValueTask.CompletedTask);

        var domainEvent = new ProductVariantCreatedDomainEvent
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Sku = "shirt-blue-m",
            Name = "Shirt / Blue / M",
            CurrentPrice = 35m,
            Currency = "USD",
            IsActive = true
        };

        await ProductVariantCreatedDomainEventHandler.Handle(domainEvent, bus.Object,
            NullLogger<ProductVariantCreatedDomainEventHandler>.Instance);

        Assert.NotNull(published);
        Assert.Equal(domainEvent.Id, published!.Id);
        Assert.Equal(domainEvent.ProductId, published.ProductId);
        Assert.Equal(domainEvent.Sku, published.Sku);
        Assert.Equal(domainEvent.CurrentPrice, published.CurrentPrice);
    }

    [Fact]
    public async Task HandleUpdated_PublishesProductVariantUpdatedEvent()
    {
        ProductVariantUpdatedEvent? published = null;
        var bus = new Mock<IMessageBus>();
        bus.Setup(x => x.PublishAsync(It.IsAny<ProductVariantUpdatedEvent>(), It.IsAny<DeliveryOptions>()))
            .Callback<ProductVariantUpdatedEvent, DeliveryOptions?>((message, _) => published = message)
            .Returns(ValueTask.CompletedTask);

        var domainEvent = new ProductVariantUpdatedDomainEvent
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Sku = "shirt-navy-m",
            Name = "Shirt / Navy / M",
            CurrentPrice = 37.5m,
            Currency = "EUR",
            IsActive = true
        };

        await ProductVariantUpdatedDomainEventHandler.Handle(domainEvent, bus.Object,
            NullLogger<ProductVariantUpdatedDomainEventHandler>.Instance);

        Assert.NotNull(published);
        Assert.Equal(domainEvent.Id, published!.Id);
        Assert.Equal(domainEvent.ProductId, published.ProductId);
        Assert.Equal(domainEvent.Sku, published.Sku);
        Assert.Equal(domainEvent.Currency, published.Currency);
    }

    [Fact]
    public async Task HandleDeleted_PublishesProductVariantDeletedEvent()
    {
        ProductVariantDeletedEvent? published = null;
        var bus = new Mock<IMessageBus>();
        bus.Setup(x => x.PublishAsync(It.IsAny<ProductVariantDeletedEvent>(), It.IsAny<DeliveryOptions>()))
            .Callback<ProductVariantDeletedEvent, DeliveryOptions?>((message, _) => published = message)
            .Returns(ValueTask.CompletedTask);

        var domainEvent = new ProductVariantDeletedDomainEvent
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Sku = "shirt-blue-m"
        };

        await ProductVariantDeletedDomainEventHandler.Handle(domainEvent, bus.Object,
            NullLogger<ProductVariantDeletedDomainEventHandler>.Instance);

        Assert.NotNull(published);
        Assert.Equal(domainEvent.Id, published!.Id);
        Assert.Equal(domainEvent.ProductId, published.ProductId);
        Assert.Equal(domainEvent.Sku, published.Sku);
    }
}
