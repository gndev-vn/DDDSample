namespace IdentityAPI.Features.Auth.Models;

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName
);