using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Features.Products.Commands;
using CatalogAPI.Features.Products.Models;
using ProductCreateModel = CatalogAPI.Features.Products.Models.ProductCreateRequest;
using ProductUpdateModel = CatalogAPI.Features.Products.Models.ProductUpdateRequest;
using FluentValidation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;

namespace DDDSample.Tests.Catalog;

public sealed class ProductCommandTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public ProductCommandTests()
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
    public async Task CreateProduct_AssignsResolvedCategory()
    {
        // Arrange
        var category = new Category("Books", "Books", "books");
        await using (var seedContext = NewContext())
        {
            seedContext.Categories.Add(category);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new CreateProductCommandHandler(dbContext);
        var command = new CreateProductCommand(new ProductCreateModel(
            "DDD Sample",
            "ddd-sample",
            "Domain-driven design sample",
            category.Id,
            42m,
            "usd"));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await using var assertContext = NewContext();
        var created = await assertContext.Products.Include(x => x.Category).SingleAsync(x => x.Id == result.Id);
        Assert.Equal(category.Id, created.Category?.Id);
        Assert.Equal(42m, result.BasePrice);
        Assert.Equal("USD", result.Currency);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateProduct_WhenCategoryMissing_ThrowsNotFound()
    {
        // Arrange
        await using var dbContext = NewContext();
        var handler = new CreateProductCommandHandler(dbContext);
        var command = new CreateProductCommand(new ProductCreateModel(
            "DDD Sample",
            "ddd-sample",
            "Domain-driven design sample",
            Guid.NewGuid(),
            42m,
            "usd"));

        // Act / Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task UpdateProduct_WhenProductExists_UpdatesFieldsAndCategory()
    {
        // Arrange
        var oldCategory = new Category("Books", "Books", "books");
        var newCategory = new Category("Patterns", "Patterns", "patterns");
        var product = new Product("DDD Sample", "Original", "ddd-sample", new Shared.ValueObjects.Money(42m, "USD"))
        {
            Category = oldCategory
        };

        await using (var seedContext = NewContext())
        {
            seedContext.Categories.AddRange(oldCategory, newCategory);
            seedContext.Products.Add(product);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new UpdateProductCommandHandler(dbContext);
        var command = new UpdateProductCommand(new ProductUpdateModel(
            product.Id,
            "Updated Sample",
            "updated-sample",
            "Updated description",
            newCategory.Id,
            99.5m,
            "eur"));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await using var assertContext = NewContext();
        var updated = await assertContext.Products.Include(x => x.Category).SingleAsync(x => x.Id == product.Id);
        Assert.Equal("Updated Sample", updated.Name);
        Assert.Equal("updated-sample", updated.Slug);
        Assert.Equal("Updated description", updated.Description);
        Assert.Equal(99.5m, updated.BasePrice!.Amount);
        Assert.Equal("EUR", updated.BasePrice!.Currency);
        Assert.Equal(newCategory.Id, updated.Category?.Id);
        Assert.Equal("EUR", result.Currency);
        Assert.Equal(99.5m, result.BasePrice);
    }

    [Fact]
    public async Task UpdateProduct_WhenProductMissing_ThrowsKeyNotFoundException()
    {
        // Arrange
        var category = new Category("Books", "Books", "books");
        await using (var seedContext = NewContext())
        {
            seedContext.Categories.Add(category);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new UpdateProductCommandHandler(dbContext);
        var command = new UpdateProductCommand(new ProductUpdateModel(
            Guid.NewGuid(),
            "Updated Sample",
            "updated-sample",
            "Updated description",
            category.Id,
            99.5m,
            "eur"));

        // Act / Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task UpdateProduct_WhenCategoryMissing_ThrowsNotFound()
    {
        // Arrange
        var existingCategory = new Category("Books", "Books", "books");
        var product = new Product("DDD Sample", "Original", "ddd-sample", new Shared.ValueObjects.Money(42m, "USD"))
        {
            Category = existingCategory
        };

        await using (var seedContext = NewContext())
        {
            seedContext.Categories.Add(existingCategory);
            seedContext.Products.Add(product);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new UpdateProductCommandHandler(dbContext);
        var command = new UpdateProductCommand(new ProductUpdateModel(
            product.Id,
            "Updated Sample",
            "updated-sample",
            "Updated description",
            Guid.NewGuid(),
            99.5m,
            "eur"));

        // Act / Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task DeleteProduct_WhenProductExists_RemovesProduct()
    {
        // Arrange
        var category = new Category("Books", "Books", "books");
        var product = new Product("DDD Sample", "Original", "ddd-sample", new Shared.ValueObjects.Money(42m, "USD"))
        {
            Category = category
        };

        await using (var seedContext = NewContext())
        {
            seedContext.Categories.Add(category);
            seedContext.Products.Add(product);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new DeleteProductCommandHandler(dbContext);

        // Act
        var deleted = await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        // Assert
        Assert.True(deleted);
        await using var assertContext = NewContext();
        Assert.False(await assertContext.Products.AnyAsync(x => x.Id == product.Id));
    }

    [Fact]
    public async Task DeleteProduct_WhenProductMissing_ThrowsKeyNotFoundException()
    {
        // Arrange
        await using var dbContext = NewContext();
        var handler = new DeleteProductCommandHandler(dbContext);

        // Act / Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new DeleteProductCommand(Guid.NewGuid()), CancellationToken.None).AsTask());
    }

    [Fact]
    public void ProductCreateValidator_WhenRequestIsInvalid_ReturnsExpectedErrors()
    {
        // Arrange
        var validator = new ProductCreateRequestValidator();
        var request = new ProductCreateModel(string.Empty, string.Empty, "desc", Guid.Empty, -1m, "us");

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductCreateModel.Name));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductCreateModel.Slug));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductCreateModel.CategoryId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductCreateModel.BasePrice));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductCreateModel.Currency));
    }

    [Fact]
    public void ProductUpdateValidator_WhenRequestIsInvalid_ReturnsExpectedErrors()
    {
        // Arrange
        var validator = new ProductUpdateRequestValidator();
        var request = new ProductUpdateModel(Guid.Empty, string.Empty, string.Empty, "desc", Guid.Empty, -1m, "us");

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductUpdateModel.Id));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductUpdateModel.Name));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductUpdateModel.Slug));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductUpdateModel.CategoryId));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductUpdateModel.BasePrice));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ProductUpdateModel.Currency));
    }
}
