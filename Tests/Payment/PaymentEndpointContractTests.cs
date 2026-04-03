using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PaymentAPI.Features;
using PaymentAPI.Features.Payments.CompletePayment;
using PaymentAPI.Features.Payments.FailPayment;
using PaymentAPI.Features.Payments.GetPaymentById;
using Shared.Authentication;
using Shared.Models;

namespace DDDSample.Tests.Payment;

public sealed class PaymentEndpointContractTests
{
    [Theory]
    [InlineData("CompletePayment", typeof(CompletePaymentRequest))]
    [InlineData("FailPayment", typeof(FailPaymentRequest))]
    public void Write_Endpoints_UseExpectedRequestContracts(string endpointName, Type requestType)
    {
        var endpoint = GetEndpointByName(endpointName);
        var acceptsMetadata = endpoint.Metadata.GetMetadata<IAcceptsMetadata>();

        Assert.NotNull(acceptsMetadata);
        Assert.Equal(requestType, acceptsMetadata!.RequestType);
    }

    [Theory]
    [InlineData("CompletePayment")]
    [InlineData("FailPayment")]
    public void Payment_Mutations_RequireManagePermission(string endpointName)
    {
        var endpoint = GetEndpointByName(endpointName);
        var authorizeData = endpoint.Metadata.OfType<IAuthorizeData>().ToList();

        Assert.Contains(authorizeData, item => item.Policy == Permissions.Payments.Manage);
    }

    [Fact]
    public void CompletePayment_DocumentsSuccessfulResponseContract()
    {
        var endpoint = GetEndpointByName("CompletePayment");
        var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();

        Assert.Contains(metadata, item =>
            item.StatusCode == StatusCodes.Status200OK &&
            item.Type == typeof(ApiResponse<PaymentModel>));
    }

    private static RouteEndpoint GetEndpointByName(string endpointName)
    {
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
        builder.Services.AddApplicationAuthorization();

        var app = builder.Build();
        app.MapPaymentEndpoints();

        return ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .Single(endpoint => endpoint.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName == endpointName);
    }
}
