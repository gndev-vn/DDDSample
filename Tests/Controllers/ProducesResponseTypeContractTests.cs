using CatalogAPI.Features;
using IdentityAPI.Features;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrderingAPI.Features;
using PaymentAPI.Features;

namespace DDDSample.Tests.Controllers;

public sealed class ProducesResponseTypeContractTests
{
    [Fact]
    public void Catalog_MinimalApi_Endpoints_Define_ProducesResponseType_Metadata()
    {
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();

        var app = builder.Build();
        app.MapCatalogEndpoints();

        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(dataSource => dataSource.Endpoints).OfType<RouteEndpoint>().ToList();

        Assert.NotEmpty(endpoints);

        foreach (var endpoint in endpoints)
        {
            var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();
            Assert.True(metadata.Count > 0,
                $"Expected minimal endpoint {endpoint.DisplayName} to define at least one produces metadata entry.");
        }
    }

    [Fact]
    public void Ordering_MinimalApi_Endpoints_Define_ProducesResponseType_Metadata()
    {
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();

        var app = builder.Build();
        app.MapOrderingEndpoints();

        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(dataSource => dataSource.Endpoints).OfType<RouteEndpoint>().ToList();

        Assert.NotEmpty(endpoints);

        foreach (var endpoint in endpoints)
        {
            var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();
            Assert.True(metadata.Count > 0,
                $"Expected minimal endpoint {endpoint.DisplayName} to define at least one produces metadata entry.");
        }
    }

    [Fact]
    public void Payment_MinimalApi_Endpoints_Define_ProducesResponseType_Metadata()
    {
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();

        var app = builder.Build();
        app.MapPaymentEndpoints();

        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(dataSource => dataSource.Endpoints).OfType<RouteEndpoint>().ToList();

        Assert.NotEmpty(endpoints);

        foreach (var endpoint in endpoints)
        {
            var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();
            Assert.True(metadata.Count > 0,
                $"Expected minimal endpoint {endpoint.DisplayName} to define at least one produces metadata entry.");
        }
    }

    [Fact]
    public void Identity_MinimalApi_Endpoints_Define_ProducesResponseType_Metadata()
    {
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();

        var app = builder.Build();
        app.MapIdentityEndpoints();

        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(dataSource => dataSource.Endpoints).OfType<RouteEndpoint>().ToList();

        Assert.NotEmpty(endpoints);

        foreach (var endpoint in endpoints)
        {
            var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();
            Assert.True(metadata.Count > 0,
                $"Expected minimal endpoint {endpoint.DisplayName} to define at least one produces metadata entry.");
        }
    }
}
