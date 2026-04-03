using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace IdentityAPI.Domain.Identity;

[CollectionName("roles")]
public class ApplicationRole : MongoIdentityRole<Guid>
{
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<string> Permissions { get; set; } = [];
}
