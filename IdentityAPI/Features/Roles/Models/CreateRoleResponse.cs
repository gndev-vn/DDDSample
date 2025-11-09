namespace IdentityAPI.Features.Roles.Models;

public record CreateRoleResponse(bool Success, string Message, Guid RoleId);