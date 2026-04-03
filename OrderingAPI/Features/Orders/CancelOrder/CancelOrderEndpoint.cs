using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace OrderingAPI.Features.Orders.CancelOrder;

public static class CancelOrderEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/cancel", async Task<Ok<ApiResponse<bool>>>(Guid id, [FromServices] IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(new CancelOrderCommand(id), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(result, "Order cancelled successfully"));
                })
            .WithName("CancelOrder")
            .WithSummary("Cancel an order")
            .WithDescription("Cancels an existing order.")
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
