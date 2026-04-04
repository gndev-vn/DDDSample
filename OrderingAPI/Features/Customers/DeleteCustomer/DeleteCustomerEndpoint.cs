using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace OrderingAPI.Features.Customers.DeleteCustomer;

public static class DeleteCustomerEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}",
                async Task<Ok<ApiResponse<object?>>>(
                    Guid id,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await mediator.Send(new DeleteCustomerCommand(id), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success<object?>(null, "Customer deleted successfully"));
                })
            .WithName("DeleteCustomer")
            .WithSummary("Delete customer")
            .WithDescription("Deletes a customer record when no orders reference it.")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
