using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Features.Products.Commands;
using Microsoft.EntityFrameworkCore;
using ProductCreateModel = CatalogAPI.Features.Products.Models.ProductCreateRequest;

namespace DDDSample.Tests.Catalog;

public sealed class ProductCommandTests
{
    [Fact]
    public async Task CreateProduct_AssignsResolvedCategory()
    {
        await using var dbContext = CreateDbContext();
        var category = new Category("Books", "Books", "books");
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();

        var handler = new CreateProductCommandHandler(dbContext);
        var command = new CreateProductCommand(new ProductCreateModel(
            "DDD Sample",
            "ddd-sample",
            "Domain-driven design sample",
            category.Id,
            42m,
            "usd"));

        var result = await handler.Handle(command, CancellationToken.None);
        var created = await dbContext.Products.Include(x => x.Category).SingleAsync(x => x.Id == result.Id);

        Assert.Equal(category.Id, created.Category?.Id);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
