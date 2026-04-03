using FluentValidation;
using IdentityAPI.Features;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Features.Auth.Login;

public static class LoginEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/login",
                async Task<IResult>(
                    [FromBody] LoginRequest request,
                    [FromServices] IValidator<LoginRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new LoginCommand(request), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(new
                    {
                        token = result.Token,
                        expiresAt = result.ExpiresAt,
                        user = result.User
                    }, result.Message));
                })
            .WithName("Login")
            .WithSummary("Authenticate a user")
            .WithDescription("Authenticates a local account and returns a JWT.")
            .Accepts<LoginRequest>("application/json")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
    }
}
