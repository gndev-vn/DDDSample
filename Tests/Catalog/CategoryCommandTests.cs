using CatalogAPI.Domain;
using CatalogAPI.Features.Categories.Commands;
using CatalogAPI.Features.Categories.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;

namespace DDDSample.Tests.Catalog;

public sealed class CategoryCommandTests
{
    [Fact]
    public async Task CreateCategory_WithMissingParent_ThrowsNotFound()
    {
        await using var dbContext = CreateDbContext();
        var handler = new CreateCategoryCommandHandler(dbContext);
        var command = new CreateCategoryCommand(new CategoryCreateRequest(
            "Child",
            "child",
            "Child category",
            Guid.NewGuid()));

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
