using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace OrderingAPI.Features.Customers.GetCustomers;

public static class GetCustomersEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet(string.Empty,
                async Task<Ok<ApiResponse<IReadOnlyList<CustomerModel>>>>(
                    [FromQuery] string? search,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(new GetCustomersQuery(search), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(result, "Customers retrieved successfully"));
                })
            .WithName("GetCustomers")
            .WithSummary("Get customers")
            .WithDescription("Retrieves customers for ordering and customer administration.")
            .Produces<ApiResponse<IReadOnlyList<CustomerModel>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);
    }
}
