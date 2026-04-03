using FluentValidation;
using OrderingAPI.Features;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderingAPI.Features.Orders.GetOrderById;
using Shared.Models;

namespace OrderingAPI.Features.Orders.UpdateOrder;

public static class UpdateOrderEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}",
                async Task<Results<Ok<ApiResponse<OrderModel>>, BadRequest<ApiResponse<object>>>>(
                    Guid id,
                    [FromBody] UpdateOrderRequest request,
                    [FromServices] IValidator<UpdateOrderRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    if (request.Id != id)
                    {
                        return TypedResults.BadRequest(ApiResponse.Error("Id in route and model id must match"));
                    }

                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new UpdateOrderCommand(new OrderModel
                    {
                        Id = request.Id,
                        CustomerId = request.CustomerId,
                        ShippingAddress = request.ShippingAddress,
                        Lines = request.Lines
                    }), cancellationToken);

                    return TypedResults.Ok(ApiResponse.Success(result, "Order updated successfully"));
                })
            .WithName("UpdateOrder")
            .WithSummary("Update an order")
            .WithDescription("Updates an existing order.")
            .Accepts<UpdateOrderRequest>("application/json")
            .Produces<ApiResponse<OrderModel>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
