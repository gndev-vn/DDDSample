using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrderingAPI.Features;
using OrderingAPI.Features.Orders.CreateOrder;
using OrderingAPI.Features.Orders.UpdateOrder;
using Shared.Authentication;

namespace DDDSample.Tests.Ordering;

public sealed class OrderingEndpointContractTests
{
    [Fact]
    public void Create_UsesOrderingCreateOrderRequestContract()
    {
        var endpoint = GetEndpointByName("CreateOrder");
        var acceptsMetadata = endpoint.Metadata.GetMetadata<IAcceptsMetadata>();
        Assert.NotNull(acceptsMetadata);
        Assert.Equal(typeof(CreateOrderRequest), acceptsMetadata!.RequestType);
    }

    [Fact]
    public void Update_UsesOrderingUpdateOrderRequestContract()
    {
        var endpoint = GetEndpointByName("UpdateOrder");
        var acceptsMetadata = endpoint.Metadata.GetMetadata<IAcceptsMetadata>();
        Assert.NotNull(acceptsMetadata);
        Assert.Equal(typeof(UpdateOrderRequest), acceptsMetadata!.RequestType);
    }

    [Theory]
    [InlineData("CreateOrder")]
    [InlineData("UpdateOrder")]
    [InlineData("PayOrder")]
    [InlineData("CancelOrder")]
    [InlineData("DeleteOrder")]
    public void Order_Mutations_RequireManagePermission(string endpointName)
    {
        var endpoint = GetEndpointByName(endpointName);
        var authorizeData = endpoint.Metadata.OfType<IAuthorizeData>().ToList();

        Assert.Contains(authorizeData, item => item.Policy == Permissions.Orders.Manage);
    }

    private static RouteEndpoint GetEndpointByName(string endpointName)
    {
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
        builder.Services.AddApplicationAuthorization();

        var app = builder.Build();
        app.MapOrderingEndpoints();

        return ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .Single(endpoint => endpoint.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName == endpointName);
    }
}
