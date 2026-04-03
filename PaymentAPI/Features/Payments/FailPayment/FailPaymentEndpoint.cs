using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PaymentAPI.Features;
using PaymentAPI.Features.Payments.GetPaymentById;
using Shared.Models;

namespace PaymentAPI.Features.Payments.FailPayment;

public static class FailPaymentEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/fail",
                async Task<Ok<ApiResponse<PaymentModel>>>(
                    Guid id,
                    [FromBody] FailPaymentRequest request,
                    [FromServices] IValidator<FailPaymentRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var payment = await mediator.Send(new FailPaymentCommand(id, request.Reason), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(payment, "Payment marked as failed"));
                })
            .WithName("FailPayment")
            .WithSummary("Fail a payment")
            .WithDescription("Marks a pending payment as failed.")
            .Accepts<FailPaymentRequest>("application/json")
            .Produces<ApiResponse<PaymentModel>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
