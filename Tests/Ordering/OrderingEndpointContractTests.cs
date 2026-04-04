using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrderingAPI.Features;
using OrderingAPI.Features.Customers.CreateCustomer;
using OrderingAPI.Features.Customers.UpdateCustomer;
using OrderingAPI.Features.Orders.CreateOrder;
using OrderingAPI.Features.Orders.UpdateOrder;
using Shared.Authentication;

namespace DDDSample.Tests.Ordering;

public sealed class OrderingEndpointContractTests
{
    [Theory]
    [InlineData("CreateCustomer", typeof(CreateCustomerRequest))]
    [InlineData("UpdateCustomer", typeof(UpdateCustomerRequest))]
    [InlineData("CreateOrder", typeof(CreateOrderRequest))]
    [InlineData("UpdateOrder", typeof(UpdateOrderRequest))]
    public void Write_Endpoints_UseExpectedRequestContracts(string endpointName, Type requestType)
    {
        var endpoint = GetEndpointByName(endpointName);
        var acceptsMetadata = endpoint.Metadata.GetMetadata<IAcceptsMetadata>();

        Assert.NotNull(acceptsMetadata);
        Assert.Equal(requestType, acceptsMetadata!.RequestType);
    }

    [Theory]
    [InlineData("CreateCustomer", Permissions.Customers.Create)]
    [InlineData("UpdateCustomer", Permissions.Customers.Update)]
    [InlineData("DeleteCustomer", Permissions.Customers.Delete)]
    [InlineData("CreateOrder", Permissions.Orders.Create)]
    [InlineData("UpdateOrder", Permissions.Orders.Update)]
    [InlineData("PayOrder", Permissions.Orders.Update)]
    [InlineData("CancelOrder", Permissions.Orders.Update)]
    [InlineData("DeleteOrder", Permissions.Orders.Delete)]
    public void Mutation_Endpoints_RequireExpectedPermission(string endpointName, string policyName)
    {
        var endpoint = GetEndpointByName(endpointName);
        var authorizeData = endpoint.Metadata.OfType<IAuthorizeData>().ToList();

        Assert.Contains(authorizeData, item => item.Policy == policyName);
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
