using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace PaymentAPI.Features.Payments.GetPaymentById;

public static class GetPaymentByIdEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}",
                async Task<Results<Ok<ApiResponse<PaymentModel>>, NotFound<ApiResponse<object>>>>(
                    Guid id,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var payment = await mediator.Send(new GetPaymentByIdQuery(id), cancellationToken);
                    if (payment is null)
                    {
                        return TypedResults.NotFound(ApiResponse.Error("Payment not found"));
                    }

                    return TypedResults.Ok(ApiResponse.Success(payment, "Payment retrieved successfully"));
                })
            .WithName("GetPaymentById")
            .WithSummary("Get a payment by id")
            .WithDescription("Retrieves a payment by identifier.")
            .Produces<ApiResponse<PaymentModel>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}