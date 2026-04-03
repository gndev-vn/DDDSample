namespace IdentityAPI.Features.Roles.CreateRole;

public record CreateRoleRequest(string Name, string Description, IEnumerable<string> Permissions);
