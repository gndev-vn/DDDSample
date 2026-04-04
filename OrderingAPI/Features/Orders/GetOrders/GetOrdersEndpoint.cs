using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderingAPI.Features.Orders.GetOrderById;
using Shared.Models;

namespace OrderingAPI.Features.Orders.GetOrders;

public static class GetOrdersEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet(string.Empty,
                async Task<Ok<ApiResponse<List<OrderModel>>>>(
                    [FromServices] IMediator mediator,
                    [FromQuery] Guid? customerId,
                    CancellationToken cancellationToken) =>
                {
                    var orders = await mediator.Send(new GetOrdersQuery(customerId), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(orders, "Orders retrieved successfully"));
                })
            .WithName("GetOrders")
            .WithSummary("Get all orders")
            .WithDescription("Retrieves all orders, optionally filtered by customer.")
            .Produces<ApiResponse<List<OrderModel>>>(StatusCodes.Status200OK);
    }
}
