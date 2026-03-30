namespace IdentityAPI.Configuration;

public sealed class IdentitySeedOptions
{
    public bool Enabled { get; set; }
    public SeedUserOptions Admin { get; set; } = new();
    public SeedUserOptions User { get; set; } = new();
}

public sealed class SeedUserOptions
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}