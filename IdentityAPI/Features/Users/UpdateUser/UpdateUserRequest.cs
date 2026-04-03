namespace IdentityAPI.Features.Users.UpdateUser;

public sealed record UpdateUserRequest(
    string Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive);
