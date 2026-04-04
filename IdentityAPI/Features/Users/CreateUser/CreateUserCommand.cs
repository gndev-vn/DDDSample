using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Users.GetUser;
using IdentityAPI.Services;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Shared.Exceptions;

namespace IdentityAPI.Features.Users.CreateUser;

public sealed record CreateUserCommand(CreateUserRequest Request) : IRequest<GetUserResponse>;

public sealed class CreateUserHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IRolePermissionService rolePermissionService) : IRequestHandler<CreateUserCommand, GetUserResponse>
{
    public async ValueTask<GetUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedRoles = request.Request.Roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedRoles.Count == 0)
        {
            throw new BusinessException("User creation failed", ["At least one valid role is required."]);
        }

        var customerId = ParseCustomerId(request.Request.CustomerId);
        if (customerId.HasValue && !normalizedRoles.Contains(IdentityRoleNames.Customer, StringComparer.OrdinalIgnoreCase))
        {
            normalizedRoles.Add(IdentityRoleNames.Customer);
        }

        var missingRoles = new List<string>();
        foreach (var role in normalizedRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                missingRoles.Add(role);
            }
        }

        if (missingRoles.Count > 0)
        {
            throw new BusinessException("User creation failed", missingRoles.Select(role => $"Role '{role}' does not exist."));
        }

        var utcNow = DateTime.UtcNow;
        var user = ApplicationUser.CreateLocal(
            request.Request.Username.Trim(),
            request.Request.Email.Trim(),
            request.Request.FirstName.Trim(),
            request.Request.LastName.Trim(),
            utcNow);
        user.IsActive = request.Request.IsActive;
        user.SetCustomerLink(customerId, utcNow);

        var createResult = await userManager.CreateAsync(user, request.Request.Password);
        if (!createResult.Succeeded)
        {
            throw new BusinessException("User creation failed", createResult.Errors.Select(error => error.Description));
        }

        var roleResult = await userManager.AddToRolesAsync(user, normalizedRoles);
        if (!roleResult.Succeeded)
        {
            throw new BusinessException("User creation failed", roleResult.Errors.Select(error => error.Description));
        }

        var permissions = await rolePermissionService.GetPermissionsAsync(normalizedRoles, cancellationToken);

        return new GetUserResponse(
            user.Id.ToString(),
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.CustomerId?.ToString(),
            normalizedRoles,
            permissions,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt);
    }

    private static Guid? ParseCustomerId(string? customerId)
        => string.IsNullOrWhiteSpace(customerId) ? null : Guid.Parse(customerId);
}
