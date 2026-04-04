using CatalogAPI.Domain.Entities;
using CatalogAPI.Domain.Events;
using Shared.Exceptions;
using Shared.ValueObjects;

namespace DDDSample.Tests.Catalog;

public sealed class CatalogBusinessLogicTests
{
    [Fact]
    public void CategoryCreate_WhenParentProvided_RaisesCreatedEventWithParentId()
    {
        var parentId = Guid.NewGuid();
        var category = new Category("Child", "Child category", "child", parentId: parentId);

        var domainEvent = Assert.IsType<CategoryCreatedDomainEvent>(Assert.Single(category.DomainEvents));
        Assert.Equal(parentId, domainEvent.ParentId);
        Assert.Equal("child", domainEvent.Slug);
    }

    [Fact]
    public void CategoryMarkDeleted_RaisesDeletedEvent()
    {
        var category = new Category("Books", "Books", "books");
        category.ClearDomainEvents();

        category.MarkDeleted();

        Assert.Contains(category.DomainEvents, x => x is CategoryDeletedDomainEvent);
    }

    [Fact]
    public void ProductMarkDeleted_RaisesDeletedEventForProductAndVariants()
    {
        var product = new Product("DDD Sample", "Domain-driven design sample", "ddd-sample", new Money(42m, "USD"));
        product.AddVariant("Size M", "DDD-SAMPLE-M", "Medium size", null, null);
        product.AddVariant("Size L", "DDD-SAMPLE-L", "Large size", null, null);
        product.ClearDomainEvents();

        product.MarkDeleted();

        Assert.Contains(product.DomainEvents, x => x is ProductDeletedDomainEvent);
        Assert.Equal(2, product.DomainEvents.OfType<ProductVariantDeletedDomainEvent>().Count());
    }

    [Fact]
    public void ProductUpdateDetails_WhenVariantUsesBasePrice_RaisesVariantUpdatedEvent()
    {
        var product = new Product("DDD Sample", "Domain-driven design sample", "ddd-sample", new Money(42m, "USD"));
        product.AddVariant("Size M", "DDD-SAMPLE-M", "Medium size", null, null);
        product.ClearDomainEvents();

        product.UpdateDetails("DDD Sample Revised", "Updated", "ddd-sample", new Money(50m, "USD"));

        var variantEvent = Assert.Single(product.DomainEvents.OfType<ProductVariantUpdatedDomainEvent>());
        Assert.Equal(50m, variantEvent.CurrentPrice);
        Assert.Equal("USD", variantEvent.Currency);
        Assert.Contains(product.DomainEvents, x => x is ProductUpdatedDomainEvent);
    }

    [Fact]
    public void CategoryUpdate_WhenParentIsSelf_ThrowsBusinessRuleException()
    {
        var category = new Category("Books", "Books", "books");

        var exception = Assert.Throws<BusinessRuleException>(() =>
            category.Update("Books", "Books", "books", true, category.Id));

        Assert.Equal("Category cannot be its own parent.", exception.Message);
    }
}
