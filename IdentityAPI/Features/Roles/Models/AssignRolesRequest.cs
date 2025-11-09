namespace IdentityAPI.Features.Roles.Models;

public record AssignRolesRequest(Guid UserId, List<string> Roles);