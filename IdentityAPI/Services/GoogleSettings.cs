using Microsoft.Extensions.Options;

namespace IdentityAPI.Services;

/// <summary>
/// Configuration options for Google OAuth / identity verification.
/// Bind from the "Google" section in appsettings.
/// </summary>
public class GoogleSettings
{
    /// <summary>
    /// The OAuth 2.0 Client ID registered in Google Cloud Console.
    /// Used to validate the audience claim of incoming Google ID tokens.
    /// Must be set to a non-empty value; omitting it will prevent the application from starting.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
}

/// <summary>
/// Validates <see cref="GoogleSettings"/> at startup so the application fails fast
/// with a clear error rather than silently rejecting every Google login at runtime.
/// </summary>
public class GoogleSettingsValidator : IValidateOptions<GoogleSettings>
{
    public ValidateOptionsResult Validate(string? name, GoogleSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            return ValidateOptionsResult.Fail(
                "Google:ClientId must be configured. " +
                "Set it via the 'Google__ClientId' environment variable or appsettings.");
        }

        return ValidateOptionsResult.Success;
    }
}
