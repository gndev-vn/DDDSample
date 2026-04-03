using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PaymentAPI.Features;
using PaymentAPI.Features.Payments.GetPaymentById;
using Shared.Models;

namespace PaymentAPI.Features.Payments.CompletePayment;

public static class CompletePaymentEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/complete",
                async Task<Ok<ApiResponse<PaymentModel>>>(
                    Guid id,
                    [FromBody] CompletePaymentRequest request,
                    [FromServices] IValidator<CompletePaymentRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var payment = await mediator.Send(new CompletePaymentCommand(id, request.TransactionReference), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(payment, "Payment completed successfully"));
                })
            .WithName("CompletePayment")
            .WithSummary("Complete a payment")
            .WithDescription("Marks a pending payment as completed.")
            .Accepts<CompletePaymentRequest>("application/json")
            .Produces<ApiResponse<PaymentModel>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
