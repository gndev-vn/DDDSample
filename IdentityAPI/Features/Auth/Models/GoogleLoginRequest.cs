namespace IdentityAPI.Features.Auth.Models;

/// <summary>
/// Request body for the Google login endpoint.
/// The client obtains an ID token from Google Sign-In (web or mobile SDK)
/// and forwards it here for server-side verification.
/// </summary>
public record GoogleLoginRequest(string IdToken);
