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
    bool IsAuthenticated { get; }
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public string? FirstName => _httpContextAccessor.HttpContext?.User?.FindFirst("firstName")?.Value;

    public string? LastName => _httpContextAccessor.HttpContext?.User?.FindFirst("lastName")?.Value;

    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value) ?? Enumerable.Empty<string>();

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}