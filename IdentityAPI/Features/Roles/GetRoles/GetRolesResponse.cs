namespace IdentityAPI.Features.Roles.GetRoles;

public sealed record GetRolesResponse(
    string Id,
    string Name,
    string Description,
    IReadOnlyList<string> Permissions,
    DateTime CreatedAt);
