using CatalogAPI.Features;
using CatalogAPI.Features.Products.CreateProduct;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DDDSample.Tests.Catalog;

public sealed class ProductControllerContractTests
{
    [Fact]
    public void Create_UsesCatalogProductCreateRequestContract()
    {
        var endpoint = GetEndpointByName("CreateProduct");

        var acceptsMetadata = endpoint.Metadata.GetMetadata<IAcceptsMetadata>();

        Assert.NotNull(acceptsMetadata);
        Assert.Equal(nameof(ProductCreateRequest), acceptsMetadata!.RequestType!.Name);
    }

    private static RouteEndpoint GetEndpointByName(string endpointName)
    {
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();

        var app = builder.Build();
        app.MapCatalogEndpoints();

        return ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .Single(endpoint => endpoint.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName == endpointName);
    }
}



