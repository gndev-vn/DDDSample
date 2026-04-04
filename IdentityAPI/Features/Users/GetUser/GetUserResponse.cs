namespace IdentityAPI.Features.Users.GetUser;

public record GetUserResponse(
    string Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string? CustomerId,
    IEnumerable<string> Roles,
    IEnumerable<string> Permissions,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
