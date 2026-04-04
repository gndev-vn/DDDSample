using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace OrderingAPI.Features.Customers.CreateCustomer;

public static class CreateCustomerEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost(string.Empty,
                async Task<Created<ApiResponse<CustomerModel>>>(
                    [FromBody] CreateCustomerRequest request,
                    [FromServices] IValidator<CreateCustomerRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(
                        new CreateCustomerCommand(request.DisplayName, request.Email, request.PhoneNumber, request.IsActive, request.Address),
                        cancellationToken);
                    return TypedResults.Created($"/api/Customers/{result.Id}", ApiResponse.Success(result, "Customer created successfully"));
                })
            .WithName("CreateCustomer")
            .WithSummary("Create customer")
            .WithDescription("Creates a customer record used by ordering operations.")
            .Accepts<CreateCustomerRequest>("application/json")
            .Produces<ApiResponse<CustomerModel>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);
    }
}
