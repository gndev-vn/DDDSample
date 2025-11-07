namespace IdentityAPI.Features.Users.Queries.GetUser;

public record GetUserResponse(
    string Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    IEnumerable<string> Roles,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);