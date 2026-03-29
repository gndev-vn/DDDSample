using System.Reflection;
using CatalogAPI.Controllers;
using CatalogAPI.Features.Products.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace DDDSample.Tests.Catalog;

public sealed class ProductVariantControllerContractTests
{
    [Fact]
    public void Create_UsesCatalogProductVariantCreateRequestContract()
    {
        var method = typeof(ProductVariantsController).GetMethod(nameof(ProductVariantsController.Create),
            BindingFlags.Instance | BindingFlags.Public)!;

        var parameterType = method.GetParameters().Single().ParameterType;

        Assert.Equal(typeof(ProductVariantCreateRequest), parameterType);
    }

    [Fact]
    public void Create_DocumentsCreatedResponse()
    {
        var method = typeof(ProductVariantsController).GetMethod(nameof(ProductVariantsController.Create),
            BindingFlags.Instance | BindingFlags.Public)!;

        var metadata = method.GetCustomAttributes<ProducesResponseTypeAttribute>().ToList();

        Assert.Contains(metadata, attribute =>
            attribute.StatusCode == StatusCodes.Status201Created &&
            attribute.Type == typeof(ApiResponse<ProductVariantResponse>));
    }

    [Fact]
    public void Delete_DocumentsNotFoundResponse()
    {
        var method = typeof(ProductVariantsController).GetMethod(nameof(ProductVariantsController.Delete),
            BindingFlags.Instance | BindingFlags.Public)!;

        var metadata = method.GetCustomAttributes<ProducesResponseTypeAttribute>().ToList();

        Assert.Contains(metadata, attribute =>
            attribute.StatusCode == StatusCodes.Status404NotFound &&
            attribute.Type == typeof(ApiResponse<object>));
    }
}
