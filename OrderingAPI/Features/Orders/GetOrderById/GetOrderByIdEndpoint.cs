using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace OrderingAPI.Features.Orders.GetOrderById;

public static class GetOrderByIdEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}",
                async Task<Results<Ok<ApiResponse<OrderModel>>, NotFound<ApiResponse<object>>>>(
                    Guid id,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var order = await mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
                    if (order is null)
                    {
                        return TypedResults.NotFound(ApiResponse.Error("Order not found"));
                    }

                    return TypedResults.Ok(ApiResponse.Success(order, "Order retrieved successfully"));
                })
            .WithName("GetOrderById")
            .WithSummary("Get an order by id")
            .WithDescription("Retrieves a single order by its identifier.")
            .Produces<ApiResponse<OrderModel>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}