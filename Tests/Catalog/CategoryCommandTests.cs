using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Features.Categories.Commands;
using CatalogAPI.Features.Categories.Models;
using FluentValidation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;

namespace DDDSample.Tests.Catalog;

public sealed class CategoryCommandTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public CategoryCommandTests()
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
    public async Task CreateCategory_WithMissingParent_ThrowsNotFound()
    {
        // Arrange
        await using var dbContext = NewContext();
        var handler = new CreateCategoryCommandHandler(dbContext);
        var command = new CreateCategoryCommand(new CategoryCreateRequest(
            "Child",
            "child",
            "Child category",
            Guid.NewGuid()));

        // Act / Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task CreateCategory_WithValidParent_PersistsParentAndReturnsCreatedCategory()
    {
        // Arrange
        var parent = new Category("Parent", "Parent category", "parent");
        await using (var seedContext = NewContext())
        {
            seedContext.Categories.Add(parent);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new CreateCategoryCommandHandler(dbContext);
        var command = new CreateCategoryCommand(new CategoryCreateRequest(
            "Child",
            "child",
            "Child category",
            parent.Id));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await using var assertContext = NewContext();
        var created = await assertContext.Categories.IgnoreQueryFilters().SingleAsync(x => x.Id == result.Id);
        Assert.Equal("Child", result.Name);
        Assert.Equal("child", result.Slug);
        Assert.Equal(parent.Id, created.ParentId);
        Assert.True(created.IsActive);
    }

    [Fact]
    public async Task UpdateCategory_WhenCategoryExists_UpdatesFieldsAndClearsParentForEmptyGuid()
    {
        // Arrange
        var parent = new Category("Parent", "Parent category", "parent");
        var child = new Category("Child", "Child description", "child", parentId: parent.Id);
        await using (var seedContext = NewContext())
        {
            seedContext.Categories.AddRange(parent, child);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new UpdateCategoryCommandHandler(dbContext);
        var command = new UpdateCategoryCommand(new CategoryUpdateRequest(
            child.Id,
            "Updated child",
            "updated-child",
            "Updated description",
            Guid.Empty,
            false));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await using var assertContext = NewContext();
        var updated = await assertContext.Categories.IgnoreQueryFilters().SingleAsync(x => x.Id == child.Id);
        Assert.Equal("Updated child", result.Name);
        Assert.Equal("updated-child", result.Slug);
        Assert.Equal("Updated description", updated.Description);
        Assert.False(updated.IsActive);
        Assert.Null(updated.ParentId);
    }

    [Fact]
    public async Task UpdateCategory_WhenCategoryMissing_ThrowsKeyNotFoundException()
    {
        // Arrange
        await using var dbContext = NewContext();
        var handler = new UpdateCategoryCommandHandler(dbContext);
        var command = new UpdateCategoryCommand(new CategoryUpdateRequest(
            Guid.NewGuid(),
            "Updated",
            "updated",
            "Description",
            Guid.Empty));

        // Act / Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task UpdateCategory_WithMissingParent_ThrowsNotFound()
    {
        // Arrange
        var category = new Category("Child", "Child category", "child");
        await using (var seedContext = NewContext())
        {
            seedContext.Categories.Add(category);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new UpdateCategoryCommandHandler(dbContext);
        var command = new UpdateCategoryCommand(new CategoryUpdateRequest(
            category.Id,
            "Child",
            "child",
            "Child category",
            Guid.NewGuid()));

        // Act / Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task DeleteCategory_WhenCategoryExists_RemovesCategory()
    {
        // Arrange
        var category = new Category("Delete me", "To delete", "delete-me");
        await using (var seedContext = NewContext())
        {
            seedContext.Categories.Add(category);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new DeleteCategoryCommandHandler(dbContext);

        // Act
        var deleted = await handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        // Assert
        Assert.True(deleted);
        await using var assertContext = NewContext();
        Assert.False(await assertContext.Categories.IgnoreQueryFilters().AnyAsync(x => x.Id == category.Id));
    }

    [Fact]
    public async Task DeleteCategory_WhenCategoryMissing_ThrowsKeyNotFoundException()
    {
        // Arrange
        await using var dbContext = NewContext();
        var handler = new DeleteCategoryCommandHandler(dbContext);

        // Act / Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new DeleteCategoryCommand(Guid.NewGuid()), CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task UpdateCategory_WhenParentIsSelf_ThrowsBusinessRuleException()
    {
        var category = new Category("Child", "Child category", "child");
        await using (var seedContext = NewContext())
        {
            seedContext.Categories.Add(category);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new UpdateCategoryCommandHandler(dbContext);
        var command = new UpdateCategoryCommand(new CategoryUpdateRequest(
            category.Id,
            "Child",
            "child",
            "Child category",
            category.Id));

        await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task UpdateCategory_WhenParentCreatesCycle_ThrowsBusinessRuleException()
    {
        var root = new Category("Root", "Root category", "root");
        var child = new Category("Child", "Child category", "child", parentId: root.Id);
        var grandchild = new Category("Grandchild", "Grandchild category", "grandchild", parentId: child.Id);

        await using (var seedContext = NewContext())
        {
            seedContext.Categories.AddRange(root, child, grandchild);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new UpdateCategoryCommandHandler(dbContext);
        var command = new UpdateCategoryCommand(new CategoryUpdateRequest(
            root.Id,
            "Root",
            "root",
            "Root category",
            grandchild.Id));

        await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task DeleteCategory_WhenChildCategoriesExist_ThrowsBusinessRuleException()
    {
        var parent = new Category("Parent", "Parent category", "parent");
        var child = new Category("Child", "Child category", "child", parentId: parent.Id);

        await using (var seedContext = NewContext())
        {
            seedContext.Categories.AddRange(parent, child);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new DeleteCategoryCommandHandler(dbContext);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(new DeleteCategoryCommand(parent.Id), CancellationToken.None).AsTask());

        Assert.Equal("Cannot delete a category that still has child categories.", exception.Message);
    }

    [Fact]
    public async Task DeleteCategory_WhenProductsExist_ThrowsBusinessRuleException()
    {
        var category = new Category("Books", "Books", "books");
        var product = new Product("DDD Sample", "Domain-driven design sample", "ddd-sample", new Shared.ValueObjects.Money(42m, "USD"))
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
        var handler = new DeleteCategoryCommandHandler(dbContext);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None).AsTask());

        Assert.Equal("Cannot delete a category that still has products.", exception.Message);
    }

    [Fact]
    public void CategoryCreateValidator_WhenRequestIsInvalid_ReturnsExpectedErrors()
    {
        // Arrange
        var validator = new CategoryCreateRequestValidator();
        var request = new CategoryCreateRequest(string.Empty, string.Empty, new string('d', 1001), null);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CategoryCreateRequest.Name));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CategoryCreateRequest.Slug));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CategoryCreateRequest.Description));
    }

    [Fact]
    public void CategoryUpdateValidator_WhenRequestIsInvalid_ReturnsExpectedErrors()
    {
        // Arrange
        var validator = new CategoryUpdateRequestValidator();
        var request = new CategoryUpdateRequest(Guid.Empty, string.Empty, string.Empty, new string('d', 1001), Guid.Empty);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CategoryUpdateRequest.Id));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CategoryUpdateRequest.Name));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CategoryUpdateRequest.Slug));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CategoryUpdateRequest.Description));
    }
}
