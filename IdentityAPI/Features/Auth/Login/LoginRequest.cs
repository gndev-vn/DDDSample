namespace IdentityAPI.Features.Auth.Login;

public record LoginRequest(
    string Email,
    string Password);
