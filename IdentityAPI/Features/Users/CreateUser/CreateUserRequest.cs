namespace IdentityAPI.Features.Users.CreateUser;

public sealed record CreateUserRequest(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    bool IsActive,
    IReadOnlyList<string> Roles);
