using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace OrderingAPI.Features.Customers.UpdateCustomer;

public static class UpdateCustomerEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}",
                async Task<Results<Ok<ApiResponse<CustomerModel>>, BadRequest<ApiResponse<object>>>>(
                    Guid id,
                    [FromBody] UpdateCustomerRequest request,
                    [FromServices] IValidator<UpdateCustomerRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    if (request.Id != id)
                    {
                        return TypedResults.BadRequest(ApiResponse.Error("Id in route and model id must match"));
                    }

                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(
                        new UpdateCustomerCommand(request.Id, request.DisplayName, request.Email, request.PhoneNumber, request.IsActive, request.Address),
                        cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(result, "Customer updated successfully"));
                })
            .WithName("UpdateCustomer")
            .WithSummary("Update customer")
            .WithDescription("Updates a customer record used by ordering operations.")
            .Accepts<UpdateCustomerRequest>("application/json")
            .Produces<ApiResponse<CustomerModel>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
