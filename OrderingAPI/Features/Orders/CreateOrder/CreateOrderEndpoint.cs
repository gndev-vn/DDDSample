using FluentValidation;
using OrderingAPI.Features;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderingAPI.Features.Orders.GetOrderById;
using Shared.Models;

namespace OrderingAPI.Features.Orders.CreateOrder;

public static class CreateOrderEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost(string.Empty,
                async Task<Created<ApiResponse<OrderModel>>>(
                    [FromBody] CreateOrderRequest request,
                    [FromServices] IValidator<CreateOrderRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new CreateOrderCommand(request.CustomerId, request.Lines, request.ShippingAddress, request.BillingAddress), cancellationToken);
                    return TypedResults.Created($"/api/Orders/{result.Id}", ApiResponse.Success(result, "Order created successfully"));
                })
            .WithName("CreateOrder")
            .WithSummary("Create an order")
            .WithDescription("Creates a new order and returns the created resource.")
            .Accepts<CreateOrderRequest>("application/json")
            .Produces<ApiResponse<OrderModel>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
