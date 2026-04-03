using CatalogAPI.Features;
using CatalogAPI.Features.Products.CreateProductVariant;
using CatalogAPI.Features.Products.GetProductVariantById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shared.Authentication;
using Shared.Models;

namespace DDDSample.Tests.Catalog;

public sealed class ProductVariantControllerContractTests
{
    [Fact]
    public void Create_UsesCatalogProductVariantCreateRequestContract()
    {
        var endpoint = GetEndpointByName("CreateProductVariant");

        var acceptsMetadata = endpoint.Metadata.GetMetadata<IAcceptsMetadata>();

        Assert.NotNull(acceptsMetadata);
        Assert.Equal(typeof(ProductVariantCreateRequest), acceptsMetadata!.RequestType);
    }

    [Fact]
    public void Create_DocumentsCreatedResponse()
    {
        var endpoint = GetEndpointByName("CreateProductVariant");

        var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();

        Assert.Contains(metadata, item =>
            item.StatusCode == StatusCodes.Status201Created &&
            item.Type == typeof(ApiResponse<ProductVariantResponse>));
    }

    [Theory]
    [InlineData("CreateProductVariant")]
    [InlineData("UpdateProductVariant")]
    [InlineData("DeleteProductVariant")]
    public void Write_Actions_RequireVariantManagePermission(string endpointName)
    {
        var endpoint = GetEndpointByName(endpointName);

        var authorizeData = endpoint.Metadata.OfType<IAuthorizeData>().ToList();

        Assert.Contains(authorizeData, item => item.Policy == Permissions.Variants.Manage);
    }

    [Fact]
    public void Delete_DocumentsNotFoundResponse()
    {
        var endpoint = GetEndpointByName("DeleteProductVariant");

        var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();

        Assert.Contains(metadata, item =>
            item.StatusCode == StatusCodes.Status404NotFound &&
            item.Type == typeof(ApiResponse<object>));
    }

    private static RouteEndpoint GetEndpointByName(string endpointName)
    {
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
        builder.Services.AddApplicationAuthorization();

        var app = builder.Build();
        app.MapCatalogEndpoints();

        return ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .Single(endpoint => endpoint.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName == endpointName);
    }
}
