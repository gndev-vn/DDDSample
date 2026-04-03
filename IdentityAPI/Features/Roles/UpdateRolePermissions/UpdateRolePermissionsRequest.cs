namespace IdentityAPI.Features.Roles.UpdateRolePermissions;

public sealed record UpdateRolePermissionsRequest(IEnumerable<string> Permissions);
