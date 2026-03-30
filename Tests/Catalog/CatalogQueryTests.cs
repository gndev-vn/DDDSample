using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Features.Categories.Queries;
using CatalogAPI.Features.Products.Queries;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.ValueObjects;

namespace DDDSample.Tests.Catalog;

public sealed class CatalogQueryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public CatalogQueryTests()
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
    public async Task GetCategoriesQuery_WhenOnlyNameFilterProvided_ReturnsMatchingCategories()
    {
        await using (var seedContext = NewContext())
        {
            seedContext.Categories.AddRange(
                new Category("Books", "Books", "books"),
                new Category("Games", "Games", "games"));
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new GetCategoriesQueryHandler(dbContext);

        var result = await handler.Handle(new GetCategoriesQuery(Name: "Book"), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Books", result[0].Name);
    }

    [Fact]
    public async Task GetProductsQuery_WhenOnlySlugFilterProvided_ReturnsMatchingProducts()
    {
        var category = new Category("Books", "Books", "books");
        var matching = new Product("DDD Sample", "Domain-driven design sample", "ddd-sample", new Money(42m, "USD"))
        {
            Category = category
        };
        var other = new Product("Patterns", "Patterns book", "patterns", new Money(50m, "USD"))
        {
            Category = category
        };

        await using (var seedContext = NewContext())
        {
            seedContext.Categories.Add(category);
            seedContext.Products.AddRange(matching, other);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new GetProductsQueryHandler(dbContext);

        var result = await handler.Handle(new GetProductsQuery(Slug: "ddd"), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(matching.Id, result[0].Id);
    }
}
