namespace IdentityAPI.Features.Roles.AssignRole;

public record AssignRolesResponse(bool Success, string Message, List<string> RoleIds);
