using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PaymentAPI.Features.Payments.GetPaymentById;
using Shared.Models;

namespace PaymentAPI.Features.Payments.GetPaymentByOrderId;

public static class GetPaymentByOrderIdEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/orders/{orderId:guid}",
                async Task<Results<Ok<ApiResponse<PaymentModel>>, NotFound<ApiResponse<object>>>>(
                    Guid orderId,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var payment = await mediator.Send(new GetPaymentByOrderIdQuery(orderId), cancellationToken);
                    if (payment is null)
                    {
                        return TypedResults.NotFound(ApiResponse.Error("Payment not found"));
                    }

                    return TypedResults.Ok(ApiResponse.Success(payment, "Payment retrieved successfully"));
                })
            .WithName("GetPaymentByOrderId")
            .WithSummary("Get a payment by order id")
            .WithDescription("Retrieves a payment using the associated order identifier.")
            .Produces<ApiResponse<PaymentModel>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}