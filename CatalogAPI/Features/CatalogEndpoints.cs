using CatalogAPI.Features.Categories.CreateCategory;
using CatalogAPI.Features.Categories.DeleteCategory;
using CatalogAPI.Features.Categories.GetCategories;
using CatalogAPI.Features.Categories.GetCategoryById;
using CatalogAPI.Features.Categories.UpdateCategory;
using CatalogAPI.Features.ProductAttributes.CreateProductAttribute;
using CatalogAPI.Features.ProductAttributes.GetProductAttributes;
using CatalogAPI.Features.Products.CreateProduct;
using CatalogAPI.Features.Products.CreateProductVariant;
using CatalogAPI.Features.Products.DeleteProduct;
using CatalogAPI.Features.Products.DeleteProductVariant;
using CatalogAPI.Features.Products.GetProductById;
using CatalogAPI.Features.Products.GetProductVariantById;
using CatalogAPI.Features.Products.GetProductVariants;
using CatalogAPI.Features.Products.GetProducts;
using CatalogAPI.Features.Products.UpdateProduct;
using CatalogAPI.Features.Products.UpdateProductVariant;
using Microsoft.AspNetCore.Authorization;
using Shared.Authentication;

namespace CatalogAPI.Features;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var categoryReads = app.MapGroup("/api/Categories")
            .WithTags("Categories")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Categories.View });
        var categoryWrites = app.MapGroup("/api/Categories")
            .WithTags("Categories")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Categories.Manage });

        GetCategoriesEndpoint.Map(categoryReads);
        GetCategoryByIdEndpoint.Map(categoryReads);
        CreateCategoryEndpoint.Map(categoryWrites);
        UpdateCategoryEndpoint.Map(categoryWrites);
        DeleteCategoryEndpoint.Map(categoryWrites);

        var productReads = app.MapGroup("/api/Products")
            .WithTags("Products")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Products.View });
        var productWrites = app.MapGroup("/api/Products")
            .WithTags("Products")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Products.Manage });

        GetProductsEndpoint.Map(productReads);
        GetProductByIdEndpoint.Map(productReads);
        CreateProductEndpoint.Map(productWrites);
        UpdateProductEndpoint.Map(productWrites);
        DeleteProductEndpoint.Map(productWrites);

        var variantReads = app.MapGroup("/api/ProductVariants")
            .WithTags("Product Variants")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Variants.View });
        var variantWrites = app.MapGroup("/api/ProductVariants")
            .WithTags("Product Variants")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Variants.Manage });

        GetProductVariantsEndpoint.Map(variantReads);
        GetProductVariantByIdEndpoint.Map(variantReads);
        CreateProductVariantEndpoint.Map(variantWrites);
        UpdateProductVariantEndpoint.Map(variantWrites);
        DeleteProductVariantEndpoint.Map(variantWrites);

        var attributeReads = app.MapGroup("/api/ProductAttributes")
            .WithTags("Product Attributes")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Variants.View });
        var attributeWrites = app.MapGroup("/api/ProductAttributes")
            .WithTags("Product Attributes")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Variants.Manage });

        GetProductAttributesEndpoint.Map(attributeReads);
        CreateProductAttributeEndpoint.Map(attributeWrites);

        return app;
    }
}
