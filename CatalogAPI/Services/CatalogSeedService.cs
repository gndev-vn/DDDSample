using CatalogAPI.Configuration;
using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.ValueObjects;

namespace CatalogAPI.Services;

public sealed class CatalogSeedService(
    AppDbContext dbContext,
    IOptions<CatalogSeedOptions> seedOptions,
    ILogger<CatalogSeedService> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!seedOptions.Value.Enabled)
        {
            logger.LogInformation("[CatalogAPI] Catalog seed is disabled.");
            return;
        }

        var categoriesBySlug = await EnsureCategoriesAsync(cancellationToken);
        var attributesByName = await EnsureProductAttributesAsync(cancellationToken);
        await EnsureProductsAsync(categoriesBySlug, attributesByName, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[CatalogAPI] Catalog seed completed.");
    }

    private async Task<Dictionary<string, Category>> EnsureCategoriesAsync(CancellationToken cancellationToken)
    {
        var categoriesBySlug = await dbContext.Categories
            .IgnoreQueryFilters()
            .ToDictionaryAsync(category => category.Slug, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var seed in CategorySeeds)
        {
            if (categoriesBySlug.ContainsKey(seed.Slug))
            {
                logger.LogInformation("[CatalogAPI] Seed category {Slug} already exists.", seed.Slug);
                continue;
            }

            Guid? parentId = null;
            if (!string.IsNullOrWhiteSpace(seed.ParentSlug))
            {
                if (!categoriesBySlug.TryGetValue(seed.ParentSlug, out var parentCategory))
                {
                    throw new InvalidOperationException($"Seed parent category '{seed.ParentSlug}' does not exist.");
                }

                parentId = parentCategory.Id;
            }

            var category = Category.Create(seed.Name, seed.Description, seed.Slug, parentId: parentId);
            category.ClearDomainEvents();

            await dbContext.Categories.AddAsync(category, cancellationToken);
            categoriesBySlug[seed.Slug] = category;
            logger.LogInformation("[CatalogAPI] Seeded category {Slug}.", seed.Slug);
        }

        return categoriesBySlug;
    }

    private async Task<Dictionary<string, ProductAttributeDefinition>> EnsureProductAttributesAsync(CancellationToken cancellationToken)
    {
        var definitionsByName = await dbContext.ProductAttributeDefinitions
            .ToDictionaryAsync(definition => definition.Name, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var attributeNames = ProductSeeds
            .SelectMany(seed => seed.Variants)
            .SelectMany(variant => variant.Attributes)
            .Select(attribute => attribute.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var attributeName in attributeNames)
        {
            if (definitionsByName.ContainsKey(attributeName))
            {
                continue;
            }

            var definition = new ProductAttributeDefinition(attributeName);
            await dbContext.ProductAttributeDefinitions.AddAsync(definition, cancellationToken);
            definitionsByName[definition.Name] = definition;
            logger.LogInformation("[CatalogAPI] Seeded product attribute definition {Name}.", definition.Name);
        }

        return definitionsByName;
    }

    private async Task EnsureProductsAsync(
        IReadOnlyDictionary<string, Category> categoriesBySlug,
        IReadOnlyDictionary<string, ProductAttributeDefinition> attributesByName,
        CancellationToken cancellationToken)
    {
        var productsBySlug = await dbContext.Products
            .Include(product => product.Variants)
            .ToDictionaryAsync(product => product.Slug, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var seed in ProductSeeds)
        {
            if (!categoriesBySlug.TryGetValue(seed.CategorySlug, out var category))
            {
                throw new InvalidOperationException($"Seed category '{seed.CategorySlug}' was not found.");
            }

            if (!productsBySlug.TryGetValue(seed.Slug, out var product))
            {
                product = Product.Create(seed.Name, seed.Description, seed.Slug, new Money(seed.BasePrice, seed.Currency));
                product.Category = category;
                product.SetImageUrl(seed.ImageUrl);

                foreach (var variantSeed in seed.Variants)
                {
                    product.AddVariant(
                        variantSeed.Name,
                        variantSeed.Sku,
                        variantSeed.Description,
                        new Money(variantSeed.OverridePrice, seed.Currency),
                        ResolveSeedAttributes(variantSeed.Attributes, attributesByName));
                }

                product.ClearDomainEvents();
                await dbContext.Products.AddAsync(product, cancellationToken);
                productsBySlug[seed.Slug] = product;
                logger.LogInformation("[CatalogAPI] Seeded product {Slug}.", seed.Slug);
                continue;
            }

            var productChanged = false;
            if (product.Category?.Id != category.Id)
            {
                product.Category = category;
                productChanged = true;
            }

            if (string.IsNullOrWhiteSpace(product.ImageUrl) && !string.IsNullOrWhiteSpace(seed.ImageUrl))
            {
                product.SetImageUrl(seed.ImageUrl);
                productChanged = true;
            }

            foreach (var variantSeed in seed.Variants)
            {
                if (product.TryGetVariantBySku(variantSeed.Sku, out _))
                {
                    continue;
                }

                product.AddVariant(
                    variantSeed.Name,
                    variantSeed.Sku,
                    variantSeed.Description,
                    new Money(variantSeed.OverridePrice, seed.Currency),
                    ResolveSeedAttributes(variantSeed.Attributes, attributesByName));

                productChanged = true;
                logger.LogInformation("[CatalogAPI] Seeded variant {Sku} for product {Slug}.", variantSeed.Sku, seed.Slug);
            }

            if (productChanged)
            {
                product.ClearDomainEvents();
            }
            else
            {
                logger.LogInformation("[CatalogAPI] Seed product {Slug} already exists.", seed.Slug);
            }
        }
    }

    private static IEnumerable<VariantAttribute> ResolveSeedAttributes(
        IReadOnlyCollection<CatalogVariantAttributeSeed> seeds,
        IReadOnlyDictionary<string, ProductAttributeDefinition> attributesByName)
    {
        return seeds.Select(seed =>
        {
            if (!attributesByName.TryGetValue(seed.Name, out var definition))
            {
                throw new InvalidOperationException($"Seed attribute definition '{seed.Name}' was not found.");
            }

            return new VariantAttribute(definition.Id, definition.Name, seed.Value);
        });
    }

    private static readonly CatalogCategorySeed[] CategorySeeds =
    [
        new("apparel", "Apparel", "Developer apparel and wearables for demos.", null),
        new("drinkware", "Drinkware", "Mugs and bottles for office-ready demo orders.", null),
        new("stationery", "Stationery", "Desk accessories and notebooks for sample orders.", null),
    ];

    private static readonly CatalogProductSeed[] ProductSeeds =
    [
        new(
            "apparel",
            "ddd-hoodie",
            "DDD Hoodie",
            "Soft fleece hoodie for domain-driven developers.",
            59.90m,
            "USD",
            "https://images.example.local/catalog/ddd-hoodie.png",
            [
                new CatalogVariantSeed(
                    "DDD Hoodie / Black / M",
                    "DDD-HOODIE-BLK-M",
                    "Black hoodie, size M.",
                    59.90m,
                    [new CatalogVariantAttributeSeed("Color", "Black"), new CatalogVariantAttributeSeed("Size", "M")]),
                new CatalogVariantSeed(
                    "DDD Hoodie / Black / L",
                    "DDD-HOODIE-BLK-L",
                    "Black hoodie, size L.",
                    59.90m,
                    [new CatalogVariantAttributeSeed("Color", "Black"), new CatalogVariantAttributeSeed("Size", "L")]),
            ]),
        new(
            "drinkware",
            "aspire-mug",
            "Aspire Mug",
            "Ceramic mug for local distributed development.",
            18.50m,
            "USD",
            "https://images.example.local/catalog/aspire-mug.png",
            [
                new CatalogVariantSeed(
                    "Aspire Mug / White / 12oz",
                    "ASPIRE-MUG-WHT-12OZ",
                    "White 12oz mug.",
                    18.50m,
                    [new CatalogVariantAttributeSeed("Color", "White"), new CatalogVariantAttributeSeed("Size", "12oz")]),
                new CatalogVariantSeed(
                    "Aspire Mug / Navy / 12oz",
                    "ASPIRE-MUG-NVY-12OZ",
                    "Navy 12oz mug.",
                    18.50m,
                    [new CatalogVariantAttributeSeed("Color", "Navy"), new CatalogVariantAttributeSeed("Size", "12oz")]),
            ]),
        new(
            "stationery",
            "clean-architecture-notebook",
            "Clean Architecture Notebook",
            "A5 dotted notebook for architecture sketches.",
            14.00m,
            "USD",
            "https://images.example.local/catalog/clean-architecture-notebook.png",
            [
                new CatalogVariantSeed(
                    "Clean Architecture Notebook / A5",
                    "NOTEBOOK-A5-DOT",
                    "A5 dotted notebook.",
                    14.00m,
                    [new CatalogVariantAttributeSeed("Format", "A5"), new CatalogVariantAttributeSeed("Pages", "160")]),
                new CatalogVariantSeed(
                    "Clean Architecture Notebook / Pocket",
                    "NOTEBOOK-POCKET-DOT",
                    "Pocket dotted notebook.",
                    11.00m,
                    [new CatalogVariantAttributeSeed("Format", "Pocket"), new CatalogVariantAttributeSeed("Pages", "128")]),
            ]),
    ];

    private sealed record CatalogCategorySeed(string Slug, string Name, string Description, string? ParentSlug);

    private sealed record CatalogProductSeed(
        string CategorySlug,
        string Slug,
        string Name,
        string Description,
        decimal BasePrice,
        string Currency,
        string ImageUrl,
        IReadOnlyCollection<CatalogVariantSeed> Variants);

    private sealed record CatalogVariantSeed(
        string Name,
        string Sku,
        string Description,
        decimal OverridePrice,
        IReadOnlyCollection<CatalogVariantAttributeSeed> Attributes);

    private sealed record CatalogVariantAttributeSeed(string Name, string Value);
}
