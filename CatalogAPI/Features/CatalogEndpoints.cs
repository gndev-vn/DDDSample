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
        var categoryCreates = app.MapGroup("/api/Categories")
            .WithTags("Categories")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Categories.Create });
        var categoryUpdates = app.MapGroup("/api/Categories")
            .WithTags("Categories")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Categories.Update });
        var categoryDeletes = app.MapGroup("/api/Categories")
            .WithTags("Categories")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Categories.Delete });

        GetCategoriesEndpoint.Map(categoryReads);
        GetCategoryByIdEndpoint.Map(categoryReads);
        CreateCategoryEndpoint.Map(categoryCreates);
        UpdateCategoryEndpoint.Map(categoryUpdates);
        DeleteCategoryEndpoint.Map(categoryDeletes);

        var productReads = app.MapGroup("/api/Products")
            .WithTags("Products")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Products.View });
        var productCreates = app.MapGroup("/api/Products")
            .WithTags("Products")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Products.Create });
        var productUpdates = app.MapGroup("/api/Products")
            .WithTags("Products")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Products.Update });
        var productDeletes = app.MapGroup("/api/Products")
            .WithTags("Products")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Products.Delete });

        GetProductsEndpoint.Map(productReads);
        GetProductByIdEndpoint.Map(productReads);
        CreateProductEndpoint.Map(productCreates);
        UpdateProductEndpoint.Map(productUpdates);
        DeleteProductEndpoint.Map(productDeletes);

        var variantReads = app.MapGroup("/api/ProductVariants")
            .WithTags("Product Variants")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Variants.View });
        var variantCreates = app.MapGroup("/api/ProductVariants")
            .WithTags("Product Variants")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Variants.Create });
        var variantUpdates = app.MapGroup("/api/ProductVariants")
            .WithTags("Product Variants")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Variants.Update });
        var variantDeletes = app.MapGroup("/api/ProductVariants")
            .WithTags("Product Variants")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Variants.Delete });

        GetProductVariantsEndpoint.Map(variantReads);
        GetProductVariantByIdEndpoint.Map(variantReads);
        CreateProductVariantEndpoint.Map(variantCreates);
        UpdateProductVariantEndpoint.Map(variantUpdates);
        DeleteProductVariantEndpoint.Map(variantDeletes);

        var attributeReads = app.MapGroup("/api/ProductAttributes")
            .WithTags("Product Attributes")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Variants.View });
        var attributeCreates = app.MapGroup("/api/ProductAttributes")
            .WithTags("Product Attributes")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Variants.Create });

        GetProductAttributesEndpoint.Map(attributeReads);
        CreateProductAttributeEndpoint.Map(attributeCreates);

        return app;
    }
}
