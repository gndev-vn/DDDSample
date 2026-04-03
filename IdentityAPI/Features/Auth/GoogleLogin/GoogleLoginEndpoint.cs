using FluentValidation;
using IdentityAPI.Features;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Features.Auth.GoogleLogin;

public static class GoogleLoginEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/google-login",
                async Task<IResult>(
                    [FromBody] GoogleLoginRequest request,
                    [FromServices] IValidator<GoogleLoginRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new GoogleLoginCommand(request), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(new
                    {
                        token = result.Token,
                        expiresAt = result.ExpiresAt,
                        user = result.User
                    }, result.Message));
                })
            .WithName("GoogleLogin")
            .WithSummary("Authenticate a user with Google")
            .WithDescription("Validates a Google ID token and returns a local JWT.")
            .Accepts<GoogleLoginRequest>("application/json")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
    }
}
