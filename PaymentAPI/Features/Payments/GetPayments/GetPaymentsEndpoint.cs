using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PaymentAPI.Features.Payments.GetPaymentById;
using Shared.Models;

namespace PaymentAPI.Features.Payments.GetPayments;

public static class GetPaymentsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet(string.Empty,
                async Task<Ok<ApiResponse<List<PaymentModel>>>>([FromServices] IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var payments = await mediator.Send(new GetPaymentsQuery(), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(payments, "Payments retrieved successfully"));
                })
            .WithName("GetPayments")
            .WithSummary("Get all payments")
            .WithDescription("Retrieves all payments.")
            .Produces<ApiResponse<List<PaymentModel>>>(StatusCodes.Status200OK);
    }
}