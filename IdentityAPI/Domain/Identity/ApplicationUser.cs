using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace IdentityAPI.Domain.Identity;

[CollectionName("users")]
public class ApplicationUser : MongoIdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Google account subject identifier ("sub" claim). Null for users who registered with a password.
    /// </summary>
    public string? GoogleId { get; set; }

    public static ApplicationUser CreateLocal(
        string username,
        string email,
        string firstName,
        string lastName,
        DateTime utcNow)
    {
        return new ApplicationUser
        {
            UserName = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }

    public static ApplicationUser CreateGoogleUser(
        string email,
        string firstName,
        string lastName,
        string googleId,
        DateTime utcNow)
    {
        return new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            GoogleId = googleId,
            EmailConfirmed = true,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }

    public void LinkGoogleAccount(string googleId, DateTime utcNow)
    {
        GoogleId = googleId;
        UpdatedAt = utcNow;
    }
}
