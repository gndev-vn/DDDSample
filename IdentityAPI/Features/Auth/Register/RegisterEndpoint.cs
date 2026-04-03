using FluentValidation;
using IdentityAPI.Features;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Features.Auth.Register;

public static class RegisterEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/register",
                async Task<IResult>(
                    [FromBody] RegisterRequest request,
                    [FromServices] IValidator<RegisterRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new RegisterCommand(request), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(new { userId = result.UserId }, result.Message));
                })
            .WithName("Register")
            .WithSummary("Register a user")
            .WithDescription("Creates a new local identity account.")
            .Accepts<RegisterRequest>("application/json")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);
    }
}
