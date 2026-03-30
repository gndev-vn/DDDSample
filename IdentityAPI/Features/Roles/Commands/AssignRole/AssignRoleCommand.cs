using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Roles.Models;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Shared.Exceptions;

namespace IdentityAPI.Features.Roles.Commands.AssignRole;

public record AssignRoleCommand(AssignRolesRequest Model) : IRequest<AssignRolesResponse>;

public record AssignRoleCommandHandler(UserManager<ApplicationUser> UserManager) : IRequestHandler<AssignRoleCommand, AssignRolesResponse>
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

        var result = await UserManager.AddToRolesAsync(user, normalizedRoles);
        if (!result.Succeeded)
        {
            throw new BusinessException("Failed to assign role", result.Errors.Select(e => e.Description));
        }

        return new AssignRolesResponse(
            Success: true,
            Message: "Roles assigned successfully",
            RoleIds: normalizedRoles.ToList()
        );
    }
}
