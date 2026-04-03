namespace IdentityAPI.Features.Roles.AssignRole;

public record AssignRolesRequest(Guid UserId, List<string> Roles);
