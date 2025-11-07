namespace IdentityAPI.Features.Auth.Models;

public record RegisterResponse(
    bool Success,
    string Message,
    string? UserId = null,
    IEnumerable<string>? Errors = null
);