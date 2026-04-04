using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Users.GetUser;
using IdentityAPI.Services;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Shared.Exceptions;

namespace IdentityAPI.Features.Users.UpdateUser;

public sealed record UpdateUserCommand(UpdateUserRequest Request) : IRequest<GetUserResponse>;

public sealed class UpdateUserHandler(
    UserManager<ApplicationUser> userManager,
    IRolePermissionService rolePermissionService) : IRequestHandler<UpdateUserCommand, GetUserResponse>
{
    public async ValueTask<GetUserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(request.Request.Id);
        if (user is null)
        {
            throw new NotFoundException("User", request.Request.Id);
        }

        user.UserName = request.Request.Username.Trim();
        user.Email = request.Request.Email.Trim();
        user.FirstName = request.Request.FirstName.Trim();
        user.LastName = request.Request.LastName.Trim();
        user.IsActive = request.Request.IsActive;
        user.SetCustomerLink(ParseCustomerId(request.Request.CustomerId), DateTime.UtcNow);

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new BusinessException("User update failed", updateResult.Errors.Select(error => error.Description));
        }

        var roles = await userManager.GetRolesAsync(user);
        var permissions = await rolePermissionService.GetPermissionsAsync(roles, cancellationToken);

        return new GetUserResponse(
            user.Id.ToString(),
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.CustomerId?.ToString(),
            roles,
            permissions,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt);
    }

    private static Guid? ParseCustomerId(string? customerId)
        => string.IsNullOrWhiteSpace(customerId) ? null : Guid.Parse(customerId);
}
