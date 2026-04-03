using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Features.Products.CreateProductVariant;
using CatalogAPI.Features.Products.DeleteProductVariant;
using CatalogAPI.Features.Products.UpdateProductVariant;
using FluentValidation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using Shared.ValueObjects;
using CreateVariantAttributeValueRequest = CatalogAPI.Features.Products.CreateProductVariant.ProductVariantAttributeValueRequest;
using UpdateVariantAttributeValueRequest = CatalogAPI.Features.Products.UpdateProductVariant.ProductVariantAttributeValueRequest;

namespace DDDSample.Tests.Catalog;

public sealed class ProductVariantCommandTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public ProductVariantCommandTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var ctx = NewContext();
        ctx.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private AppDbContext NewContext() => new(_options);

    [Fact]
    public async Task CreateProductVariant_AssignsParentAndPersistsAttributes()
    {
        var color = new ProductAttributeDefinition("Color");
        var size = new ProductAttributeDefinition("Size");
        var product = new Product("Shirt", "Cotton shirt", "shirt", new Money(30m, "USD"));

        await using (var seedContext = NewContext())
        {
            seedContext.ProductAttributeDefinitions.AddRange(color, size);
            seedContext.Products.Add(product);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new CreateProductVariantCommandHandler(dbContext);
        var command = new CreateProductVariantCommand(new ProductVariantCreateRequest(
            "Shirt / Blue / M",
            "shirt-blue-m",
            "Blue medium shirt",
            product.Id,
            35m,
            "usd",
            [
                new CreateVariantAttributeValueRequest(color.Id, "Blue"),
                new CreateVariantAttributeValueRequest(size.Id, "M"),
            ]));

        var result = await handler.Handle(command, CancellationToken.None);

        await using var assertContext = NewContext();
        var created = await assertContext.ProductVariants.SingleAsync(x => x.Id == result.Id);
        Assert.Equal(product.Id, created.ProductId);
        Assert.Equal("shirt-blue-m", created.Sku);
        Assert.Equal(35m, created.OverridePrice!.Amount);
        Assert.Equal("USD", created.OverridePrice.Currency);
        Assert.Equal(2, created.Attributes.Count);
        Assert.Contains(created.Attributes, attribute => attribute.AttributeId == color.Id && attribute.Value == "Blue");
        Assert.Equal("USD", result.Currency);
        Assert.Equal(35m, result.OverridePrice);
    }

    [Fact]
    public async Task CreateProductVariant_WhenParentMissing_ThrowsNotFound()
    {
        await using var dbContext = NewContext();
        var handler = new CreateProductVariantCommandHandler(dbContext);
        var command = new CreateProductVariantCommand(new ProductVariantCreateRequest(
            "Shirt / Blue / M",
            "shirt-blue-m",
            "Blue medium shirt",
            Guid.NewGuid(),
            35m,
            "usd",
            []));

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task UpdateProductVariant_WhenVariantExists_UpdatesFieldsAndAttributes()
    {
        var color = new ProductAttributeDefinition("Color");
        var size = new ProductAttributeDefinition("Size");
        var product = new Product("Shirt", "Cotton shirt", "shirt", new Money(30m, "USD"));
        var variant = product.AddVariant(
            "Shirt / Blue / M",
            "shirt-blue-m",
            "Blue medium shirt",
            new Money(35m, "USD"),
            [new VariantAttribute(color.Id, color.Name, "Blue")]);

        await using (var seedContext = NewContext())
        {
            seedContext.ProductAttributeDefinitions.AddRange(color, size);
            seedContext.Products.Add(product);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new UpdateProductVariantCommandHandler(dbContext);
        var command = new UpdateProductVariantCommand(new ProductVariantUpdateRequest(
            variant.Id,
            "Shirt / Navy / M",
            "shirt-navy-m",
            "Navy medium shirt",
            product.Id,
            37.5m,
            "eur",
            [
                new UpdateVariantAttributeValueRequest(color.Id, "Navy"),
                new UpdateVariantAttributeValueRequest(size.Id, "M"),
            ]));

        var result = await handler.Handle(command, CancellationToken.None);

        await using var assertContext = NewContext();
        var updated = await assertContext.ProductVariants.SingleAsync(x => x.Id == variant.Id);
        Assert.Equal("Shirt / Navy / M", updated.Name);
        Assert.Equal("shirt-navy-m", updated.Sku);
        Assert.Equal("Navy medium shirt", updated.Description);
        Assert.Equal(37.5m, updated.OverridePrice!.Amount);
        Assert.Equal("EUR", updated.OverridePrice.Currency);
        Assert.Equal(2, updated.Attributes.Count);
        Assert.Contains(updated.Attributes, attribute => attribute.AttributeId == color.Id && attribute.Value == "Navy");
        Assert.Contains(updated.Attributes, attribute => attribute.AttributeId == size.Id && attribute.Value == "M");
        Assert.Equal("EUR", result.Currency);
        Assert.Equal(37.5m, result.OverridePrice);
    }

    [Fact]
    public async Task UpdateProductVariant_WhenParentMissing_ThrowsNotFound()
    {
        var color = new ProductAttributeDefinition("Color");
        var product = new Product("Shirt", "Cotton shirt", "shirt", new Money(30m, "USD"));
        var variant = product.AddVariant(
            "Shirt / Blue / M",
            "shirt-blue-m",
            "Blue medium shirt",
            new Money(35m, "USD"),
            [new VariantAttribute(color.Id, color.Name, "Blue")]);

        await using (var seedContext = NewContext())
        {
            seedContext.ProductAttributeDefinitions.Add(color);
            seedContext.Products.Add(product);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new UpdateProductVariantCommandHandler(dbContext);
        var command = new UpdateProductVariantCommand(new ProductVariantUpdateRequest(
            variant.Id,
            "Shirt / Navy / M",
            "shirt-navy-m",
            "Navy medium shirt",
            Guid.NewGuid(),
            37.5m,
            "eur",
            [new UpdateVariantAttributeValueRequest(color.Id, "Navy")]));

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task DeleteProductVariant_WhenVariantExists_RemovesVariant()
    {
        var color = new ProductAttributeDefinition("Color");
        var product = new Product("Shirt", "Cotton shirt", "shirt", new Money(30m, "USD"));
        var variant = product.AddVariant(
            "Shirt / Blue / M",
            "shirt-blue-m",
            "Blue medium shirt",
            new Money(35m, "USD"),
            [new VariantAttribute(color.Id, color.Name, "Blue")]);

        await using (var seedContext = NewContext())
        {
            seedContext.ProductAttributeDefinitions.Add(color);
            seedContext.Products.Add(product);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new DeleteProductVariantCommandHandler(dbContext);

        var deleted = await handler.Handle(new DeleteProductVariantCommand(variant.Id), CancellationToken.None);

        Assert.True(deleted);
        await using var assertContext = NewContext();
        Assert.False(await assertContext.ProductVariants.AnyAsync(x => x.Id == variant.Id));
    }

    [Fact]
    public async Task DeleteProductVariant_WhenVariantMissing_ThrowsKeyNotFoundException()
    {
        await using var dbContext = NewContext();
        var handler = new DeleteProductVariantCommandHandler(dbContext);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new DeleteProductVariantCommand(Guid.NewGuid()), CancellationToken.None).AsTask());
    }

    [Fact]
    public void ProductVariantCreateValidator_WhenRequestIsInvalid_ReturnsExpectedErrors()
    {
        var validator = new ProductVariantCreateRequestValidator();
        var request = new ProductVariantCreateRequest(
            string.Empty,
            string.Empty,
            new string('x', 5000),
            Guid.Empty,
            -1m,
            "us",
            [
                new CreateVariantAttributeValueRequest(Guid.Empty, string.Empty),
                new CreateVariantAttributeValueRequest(Guid.Empty, string.Empty),
            ]);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductVariantCreateRequest.Name));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductVariantCreateRequest.Sku));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductVariantCreateRequest.ParentId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductVariantCreateRequest.OverridePrice));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductVariantCreateRequest.Currency));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductVariantCreateRequest.Attributes));
    }

    [Fact]
    public void ProductVariantUpdateValidator_WhenRequestIsInvalid_ReturnsExpectedErrors()
    {
        var validator = new ProductVariantUpdateRequestValidator();
        var request = new ProductVariantUpdateRequest(
            Guid.Empty,
            string.Empty,
            string.Empty,
            new string('x', 5000),
            Guid.Empty,
            -1m,
            "us",
            [new UpdateVariantAttributeValueRequest(Guid.Empty, string.Empty)]);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductVariantUpdateRequest.Id));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductVariantUpdateRequest.Name));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductVariantUpdateRequest.Sku));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductVariantUpdateRequest.ParentId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductVariantUpdateRequest.OverridePrice));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductVariantUpdateRequest.Currency));
        Assert.Contains(result.Errors, x => x.PropertyName == "Attributes[0].AttributeId");
        Assert.Contains(result.Errors, x => x.PropertyName == "Attributes[0].Value");
    }
}


