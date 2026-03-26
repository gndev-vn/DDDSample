using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace IdentityAPI.Services;

/// <summary>
/// Verified identity payload returned after a successful Google ID token validation.
/// </summary>
public record GoogleUserInfo(
    string Subject,
    string Email,
    string? GivenName,
    string? FamilyName);

/// <summary>
/// Validates Google ID tokens and returns a strongly-typed identity payload.
/// Abstracts the Google.Apis.Auth SDK so handlers remain SDK-agnostic.
/// </summary>
public interface IGoogleTokenValidator
{
    /// <summary>
    /// Validates the supplied Google ID token against Google's public keys and the
    /// configured audience. Throws <see cref="UnauthorizedAccessException"/> when the
    /// token is invalid or expired.
    /// </summary>
    Task<GoogleUserInfo> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly GoogleSettings _settings;
    private readonly GoogleJsonWebSignature.ValidationSettings _validationSettings;

    public GoogleTokenValidator(IOptions<GoogleSettings> settings)
    {
        _settings = settings.Value;
        _validationSettings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [_settings.ClientId]
        };
    }

    public async Task<GoogleUserInfo> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, _validationSettings);
        }
        catch (InvalidJwtException ex)
        {
            throw new UnauthorizedAccessException("Invalid or expired Google token.", ex);
        }

        // The Google.Apis.Auth library does NOT automatically enforce email_verified.
        // An unverified email must be rejected: without this check an attacker could
        // create a Google account with an unverified email that matches an existing
        // local account and use it to take over that account.
        if (payload.EmailVerified != true)
        {
            throw new UnauthorizedAccessException(
                "Google account email address is not verified. Please verify your Google account email before signing in.");
        }

        return new GoogleUserInfo(
            Subject: payload.Subject,
            Email: payload.Email,
            GivenName: payload.GivenName,
            FamilyName: payload.FamilyName);
    }
}
