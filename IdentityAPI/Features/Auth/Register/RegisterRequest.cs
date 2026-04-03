namespace IdentityAPI.Features.Auth.Register;

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName
);
