using System.Reflection;
using CatalogAPI.Controllers;
using ProductCreateModel = CatalogAPI.Features.Products.Models.ProductCreateRequest;

namespace DDDSample.Tests.Catalog;

public sealed class ProductControllerContractTests
{
    [Fact]
    public void Create_UsesCatalogProductCreateRequestContract()
    {
        // Arrange
        var method = typeof(ProductsController).GetMethod(nameof(ProductsController.Create), BindingFlags.Instance | BindingFlags.Public)!;

        // Act
        var parameterType = method.GetParameters().Single().ParameterType;

        // Assert
        Assert.Equal(typeof(ProductCreateModel), parameterType);
    }
}
