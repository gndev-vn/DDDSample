using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace OrderingAPI.Features.Orders.PayOrder;

public static class PayOrderEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/pay", async Task<Ok<ApiResponse<bool>>>(Guid id, [FromServices] IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(new PayOrderCommand(id), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(result, "Order paid successfully"));
                })
            .WithName("PayOrder")
            .WithSummary("Pay an order")
            .WithDescription("Marks an order as paid.")
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
