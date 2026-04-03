using CatalogAPI.Configuration;
using CatalogAPI.Domain;
using CatalogAPI.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DDDSample.Tests.Catalog;

public sealed class CatalogSeedServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public CatalogSeedServiceTests()
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
    public async Task SeedAsync_CreatesCatalogDemoData_WithoutDuplicates()
    {
        await using var seedContext = NewContext();
        var service = new CatalogSeedService(
            seedContext,
            Options.Create(new CatalogSeedOptions { Enabled = true }),
            NullLogger<CatalogSeedService>.Instance);

        await service.SeedAsync();
        await service.SeedAsync();

        await using var assertContext = NewContext();
        var categories = await assertContext.Categories.IgnoreQueryFilters().ToListAsync();
        var products = await assertContext.Products.Include(product => product.Variants).ToListAsync();

        Assert.Equal(3, categories.Count);
        Assert.Equal(3, products.Count);
        Assert.Equal(6, products.SelectMany(product => product.Variants).Count());
        Assert.Contains(products, product => product.Slug == "ddd-hoodie" && !string.IsNullOrWhiteSpace(product.ImageUrl));
        Assert.Contains(products.SelectMany(product => product.Variants), variant => variant.Sku == "ASPIRE-MUG-WHT-12OZ");
    }
}
