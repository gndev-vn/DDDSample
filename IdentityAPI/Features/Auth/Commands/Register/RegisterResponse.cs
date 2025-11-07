namespace IdentityAPI.Features.Auth.Commands.Register;

public record RegisterResponse(
    bool Success,
    string Message,
    string? UserId = null,
    IEnumerable<string>? Errors = null
);