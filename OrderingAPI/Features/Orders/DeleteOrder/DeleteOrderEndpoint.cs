using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace OrderingAPI.Features.Orders.DeleteOrder;

public static class DeleteOrderEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", async Task<Ok<ApiResponse<string>>>(Guid id, [FromServices] IMediator mediator, CancellationToken cancellationToken) =>
                {
                    await mediator.Send(new DeleteOrderCommand(id), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success<string>("Order deleted successfully"));
                })
            .WithName("DeleteOrder")
            .WithSummary("Delete an order")
            .WithDescription("Deletes an existing order.")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
