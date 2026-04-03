using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Shared.Authentication;

public interface ICurrentUser
{
    string? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    string? FirstName { get; }
    string? LastName { get; }
    IEnumerable<string> Roles { get; }
    IEnumerable<string> Permissions { get; }
    bool IsAuthenticated { get; }
}

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string? UserId => httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? Username => httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Email => httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public string? FirstName => httpContextAccessor.HttpContext?.User?.FindFirst("firstName")?.Value;

    public string? LastName => httpContextAccessor.HttpContext?.User?.FindFirst("lastName")?.Value;

    public IEnumerable<string> Roles => httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value) ?? Enumerable.Empty<string>();

    public IEnumerable<string> Permissions => httpContextAccessor.HttpContext?.User?.FindAll(PermissionClaimTypes.Permission)?.Select(c => c.Value) ?? Enumerable.Empty<string>();

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
