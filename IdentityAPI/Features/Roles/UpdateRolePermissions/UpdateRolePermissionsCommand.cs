using IdentityAPI.Domain.Identity;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Shared.Exceptions;

namespace IdentityAPI.Features.Roles.UpdateRolePermissions;

public sealed record UpdateRolePermissionsCommand(string RoleId, UpdateRolePermissionsRequest Model) : IRequest<UpdateRolePermissionsResponse>;

public sealed class UpdateRolePermissionsCommandHandler(RoleManager<ApplicationRole> roleManager) : IRequestHandler<UpdateRolePermissionsCommand, UpdateRolePermissionsResponse>
{
    public async ValueTask<UpdateRolePermissionsResponse> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await roleManager.FindByIdAsync(request.RoleId);
        if (role is null)
        {
            throw new NotFoundException("Role not found", request.RoleId);
        }

        role.Permissions = request.Model.Permissions
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Select(permission => permission.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(permission => permission)
            .ToList();

        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            throw new BusinessException("Failed to update role permissions", result.Errors.Select(error => error.Description));
        }

        return new UpdateRolePermissionsResponse(true, "Role permissions updated successfully", role.Id.ToString());
    }
}
