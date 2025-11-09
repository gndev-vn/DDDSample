namespace IdentityAPI.Features.Roles.Models;

public record AssignRolesResponse(bool Success, string Message, List<string> RoleIds);