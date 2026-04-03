using IdentityAPI.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace IdentityAPI.Services;

public interface IRolePermissionService
{
    Task<IReadOnlyList<string>> GetPermissionsAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken);
}

public sealed class RolePermissionService(RoleManager<ApplicationRole> roleManager) : IRolePermissionService
{
    public async Task<IReadOnlyList<string>> GetPermissionsAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken)
    {
        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in roleNames.Where(role => !string.IsNullOrWhiteSpace(role)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var role = await roleManager.FindByNameAsync(roleName.Trim());
            if (role is null)
            {
                continue;
            }

            foreach (var permission in role.Permissions.Where(permission => !string.IsNullOrWhiteSpace(permission)))
            {
                permissions.Add(permission.Trim());
            }
        }

        return permissions.OrderBy(permission => permission).ToArray();
    }
}
