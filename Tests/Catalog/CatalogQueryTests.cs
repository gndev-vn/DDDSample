using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using CatalogAPI.Features.Categories.GetCategories;
using CatalogAPI.Features.Products.GetProductVariantById;
using CatalogAPI.Features.Products.GetProductVariants;
using CatalogAPI.Features.Products.GetProducts;
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
            Category = category,
        };
        var other = new Product("Patterns", "Patterns book", "patterns", new Money(50m, "USD"))
        {
            Category = category,
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

    [Fact]
    public async Task GetProductVariantsQuery_WithOwnedOverridePrice_ReturnsProjectedVariants()
    {
        var color = new ProductAttributeDefinition("Color");
        var size = new ProductAttributeDefinition("Size");
        var product = new Product("Shirt", "Cotton shirt", "shirt", new Money(30m, "USD"));
        var variant = product.AddVariant(
            "Shirt / Blue / M",
            "shirt-blue-m",
            "Blue medium shirt",
            new Money(35m, "USD"),
            [
                new VariantAttribute(color.Id, color.Name, "Blue"),
                new VariantAttribute(size.Id, size.Name, "M"),
            ]);

        await using (var seedContext = NewContext())
        {
            seedContext.ProductAttributeDefinitions.AddRange(color, size);
            seedContext.Products.Add(product);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new GetProductVariantsQueryHandler(dbContext);

        var result = await handler.Handle(new GetProductVariantsQuery(), CancellationToken.None);

        var response = Assert.Single(result);
        Assert.Equal(variant.Id, response.Id);
        Assert.Equal(35m, response.OverridePrice);
        Assert.Equal("USD", response.Currency);
        Assert.Equal(2, response.Attributes.Count);
        Assert.Contains(response.Attributes, attribute => attribute.AttributeId == color.Id && attribute.Name == "Color");
    }

    [Fact]
    public async Task GetProductVariantByIdQuery_WithOwnedOverridePrice_ReturnsProjectedVariant()
    {
        var color = new ProductAttributeDefinition("Color");
        var product = new Product("Hat", "Wool hat", "hat", new Money(20m, "USD"));
        var variant = product.AddVariant(
            "Hat / Black",
            "hat-black",
            "Black wool hat",
            new Money(24m, "USD"),
            [new VariantAttribute(color.Id, color.Name, "Black")]);

        await using (var seedContext = NewContext())
        {
            seedContext.ProductAttributeDefinitions.Add(color);
            seedContext.Products.Add(product);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new GetProductVariantByIdQueryHandler(dbContext);

        var result = await handler.Handle(new GetProductVariantByIdQuery(variant.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(variant.Id, result!.Id);
        Assert.Equal(24m, result.OverridePrice);
        Assert.Equal("USD", result.Currency);
        var attribute = Assert.Single(result.Attributes);
        Assert.Equal(color.Id, attribute.AttributeId);
        Assert.Equal("Color", attribute.Name);
        Assert.Equal("Black", attribute.Value);
    }
}
