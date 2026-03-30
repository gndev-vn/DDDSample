using FluentValidation.TestHelper;
using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Auth.Commands.GoogleLogin;
using IdentityAPI.Features.Auth.Models;
using IdentityAPI.Features.Auth.Services;
using IdentityAPI.Services;
using Moq;

namespace DDDSample.Tests.Identity;

public sealed class GoogleLoginHandlerTests
{
    private static GoogleUserInfo ValidGoogleUser(string email = "alice@example.com") =>
        new("google-sub-123", email, "Alice", "Smith");

    [Fact]
    public async Task Handle_ValidToken_ResolvedAccount_ReturnsJwt()
    {
        var googleUser = ValidGoogleUser();
        var existingUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = googleUser.Email,
            Email = googleUser.Email,
            FirstName = "Alice",
            LastName = "Smith",
            GoogleId = googleUser.Subject
        };
        var resolution = new IdentityAccountResolution(existingUser, ["User"]);
        var expectedResponse = new LoginResponse(true, "Login successful", "signed-jwt-token");

        var validator = new Mock<IGoogleTokenValidator>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(googleUser);

        var identityAccountService = new Mock<IIdentityAccountService>();
        identityAccountService
            .Setup(service => service.ResolveGoogleAccountAsync(googleUser, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resolution);

        var loginResponseFactory = new Mock<ILoginResponseFactory>();
        loginResponseFactory
            .Setup(factory => factory.CreateAsync(existingUser, resolution.Roles, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var handler = new GoogleLoginHandler(identityAccountService.Object, loginResponseFactory.Object, validator.Object);

        var result = await handler.Handle(new GoogleLoginCommand(new GoogleLoginRequest("id-token")), default);

        Assert.Same(expectedResponse, result);
        Assert.Equal("signed-jwt-token", result.Token);
        identityAccountService.Verify(service => service.ResolveGoogleAccountAsync(googleUser, It.IsAny<CancellationToken>()), Times.Once);
        loginResponseFactory.Verify(factory => factory.CreateAsync(existingUser, resolution.Roles, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidToken_ThrowsUnauthorizedAccessException()
    {
        var validator = new Mock<IGoogleTokenValidator>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid or expired Google token."));

        var identityAccountService = new Mock<IIdentityAccountService>();
        var loginResponseFactory = new Mock<ILoginResponseFactory>();
        var handler = new GoogleLoginHandler(identityAccountService.Object, loginResponseFactory.Object, validator.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new GoogleLoginCommand(new GoogleLoginRequest("bad-token")), default).AsTask());

        identityAccountService.Verify(service => service.ResolveGoogleAccountAsync(It.IsAny<GoogleUserInfo>(), It.IsAny<CancellationToken>()), Times.Never);
        loginResponseFactory.Verify(factory => factory.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidToken_UnverifiedEmail_ThrowsUnauthorizedAccessException()
    {
        var validator = new Mock<IGoogleTokenValidator>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException(
                "Google account email address is not verified. Please verify your Google account email before signing in."));

        var identityAccountService = new Mock<IIdentityAccountService>();
        var loginResponseFactory = new Mock<ILoginResponseFactory>();
        var handler = new GoogleLoginHandler(identityAccountService.Object, loginResponseFactory.Object, validator.Object);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new GoogleLoginCommand(new GoogleLoginRequest("unverified-token")), default).AsTask());

        Assert.Contains("not verified", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class GoogleLoginValidatorTests
{
    private readonly GoogleLoginValidator _validator = new();

    [Fact]
    public void Validate_EmptyIdToken_HasValidationError()
    {
        var result = _validator.TestValidate(new GoogleLoginRequest(string.Empty));
        result.ShouldHaveValidationErrorFor(x => x.IdToken)
            .WithErrorMessage("Google ID token is required.");
    }

    [Fact]
    public void Validate_NullIdToken_HasValidationError()
    {
        var result = _validator.TestValidate(new GoogleLoginRequest(null!));
        result.ShouldHaveValidationErrorFor(x => x.IdToken);
    }

    [Fact]
    public void Validate_NonEmptyIdToken_PassesValidation()
    {
        var result = _validator.TestValidate(new GoogleLoginRequest("eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.some-payload.sig"));
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public sealed class GoogleSettingsValidatorTests
{
    private readonly GoogleSettingsValidator _validator = new();

    [Fact]
    public void Validate_EmptyClientId_ReturnsFailure()
    {
        var result = _validator.Validate(null, new GoogleSettings { ClientId = string.Empty });
        Assert.True(result.Failed);
        Assert.Contains("Google:ClientId must be configured", result.FailureMessage);
    }

    [Fact]
    public void Validate_WhitespaceClientId_ReturnsFailure()
    {
        var result = _validator.Validate(null, new GoogleSettings { ClientId = "   " });
        Assert.True(result.Failed);
    }

    [Fact]
    public void Validate_ValidClientId_ReturnsSuccess()
    {
        var result = _validator.Validate(null, new GoogleSettings
        {
            ClientId = "123456789-abc.apps.googleusercontent.com"
        });
        Assert.False(result.Failed);
    }
}
