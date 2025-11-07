namespace IdentityAPI.Features.Auth.Commands.Login;

public record LoginResponse(
    bool Success,
    string Message,
    string? Token = null,
    DateTime? ExpiresAt = null,
    UserInfo? User = null
);

public record UserInfo(
    string Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    IEnumerable<string> Roles
);