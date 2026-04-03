using IdentityAPI.Domain.Identity;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Shared.Exceptions;

namespace IdentityAPI.Features.Roles.AssignRole;

public record AssignRoleCommand(AssignRolesRequest Model) : IRequest<AssignRolesResponse>;

public record AssignRoleCommandHandler(
    UserManager<ApplicationUser> UserManager,
    RoleManager<ApplicationRole> RoleManager) : IRequestHandler<AssignRoleCommand, AssignRolesResponse>
{
    public async ValueTask<AssignRolesResponse> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await UserManager.FindByIdAsync(request.Model.UserId.ToString());
        if (user == null)
        {
            throw new NotFoundException("User not found", request.Model.UserId);
        }

        var normalizedRoles = request.Model.Roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var missingRoles = new List<string>();
        foreach (var role in normalizedRoles)
        {
            if (!await RoleManager.RoleExistsAsync(role))
            {
                missingRoles.Add(role);
            }
        }

        if (missingRoles.Count > 0)
        {
            throw new BusinessException("Failed to assign role", missingRoles.Select(role => $"Role '{role}' does not exist."));
        }

        var currentRoles = await UserManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles
            .Except(normalizedRoles, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (rolesToRemove.Length > 0)
        {
            var removeResult = await UserManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                throw new BusinessException("Failed to remove user roles", removeResult.Errors.Select(e => e.Description));
            }
        }

        var rolesToAdd = normalizedRoles
            .Except(currentRoles, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (rolesToAdd.Length > 0)
        {
            var addResult = await UserManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                throw new BusinessException("Failed to assign role", addResult.Errors.Select(e => e.Description));
            }
        }

        return new AssignRolesResponse(
            Success: true,
            Message: "Roles updated successfully",
            RoleIds: normalizedRoles.ToList()
        );
    }
}
