using FluentValidation.TestHelper;
using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Auth.Commands.GoogleLogin;
using IdentityAPI.Features.Auth.Models;
using IdentityAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Exceptions;
using Shared.Models;

namespace DDDSample.Tests.Identity;

public sealed class GoogleLoginHandlerTests
{
    // ── helpers ────────────────────────────────────────────────────────────────

    private static Mock<UserManager<ApplicationUser>> BuildUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<ApplicationUser>>().Object,
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
    }

    private static IOptions<JwtSettings> JwtOptions() =>
        Options.Create(new JwtSettings
        {
            SecretKey = "unit-test-secret-key-32chars-long!!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationInMinutes = 60
        });

    private static GoogleUserInfo ValidGoogleUser(string email = "alice@example.com") =>
        new("google-sub-123", email, "Alice", "Smith");

    private static ApplicationUser ActiveUser(string email = "alice@example.com") =>
        new()
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = "Alice",
            LastName = "Smith",
            IsActive = true
        };

    // ── handler tests ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidToken_ExistingActiveUser_ReturnsJwt()
    {
        // Arrange
        var googleUser = ValidGoogleUser();
        var existingUser = ActiveUser();
        existingUser.GoogleId = "google-sub-123";

        var userManager = BuildUserManagerMock();
        userManager.Setup(m => m.FindByEmailAsync(googleUser.Email))
            .ReturnsAsync(existingUser);
        userManager.Setup(m => m.GetRolesAsync(existingUser))
            .ReturnsAsync(["User"]);

        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.GenerateTokenAsync(existingUser))
            .ReturnsAsync("signed-jwt-token");

        var validator = new Mock<IGoogleTokenValidator>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(googleUser);

        var handler = new GoogleLoginHandler(userManager.Object, jwtService.Object, validator.Object, JwtOptions());

        // Act
        var result = await handler.Handle(new GoogleLoginCommand(new GoogleLoginRequest("id-token")), default);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("signed-jwt-token", result.Token);
        Assert.Equal(existingUser.Email, result.User!.Email);
        Assert.Contains("User", result.User.Roles);
    }

    [Fact]
    public async Task Handle_ValidToken_ExistingUserWithoutGoogleId_LinksGoogleIdAndReturnsJwt()
    {
        // Arrange
        var googleUser = ValidGoogleUser();
        var existingUser = ActiveUser();
        existingUser.GoogleId = null; // password-only account not yet linked

        var userManager = BuildUserManagerMock();
        userManager.Setup(m => m.FindByEmailAsync(googleUser.Email))
            .ReturnsAsync(existingUser);
        userManager.Setup(m => m.UpdateAsync(existingUser))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.GetRolesAsync(existingUser))
            .ReturnsAsync(["User"]);

        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.GenerateTokenAsync(existingUser))
            .ReturnsAsync("signed-jwt-token");

        var validator = new Mock<IGoogleTokenValidator>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(googleUser);

        var handler = new GoogleLoginHandler(userManager.Object, jwtService.Object, validator.Object, JwtOptions());

        // Act
        await handler.Handle(new GoogleLoginCommand(new GoogleLoginRequest("id-token")), default);

        // Assert: GoogleId was set and UpdateAsync was called exactly once
        Assert.Equal("google-sub-123", existingUser.GoogleId);
        userManager.Verify(m => m.UpdateAsync(existingUser), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidToken_NewUser_ProvisionesUserAndReturnsJwt()
    {
        // Arrange
        var googleUser = ValidGoogleUser("newuser@example.com");

        var userManager = BuildUserManagerMock();
        userManager.Setup(m => m.FindByEmailAsync(googleUser.Email))
            .ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(["User"]);

        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.GenerateTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("new-user-jwt");

        var validator = new Mock<IGoogleTokenValidator>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(googleUser);

        var handler = new GoogleLoginHandler(userManager.Object, jwtService.Object, validator.Object, JwtOptions());

        // Act
        var result = await handler.Handle(new GoogleLoginCommand(new GoogleLoginRequest("id-token")), default);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("new-user-jwt", result.Token);
        userManager.Verify(m => m.CreateAsync(It.Is<ApplicationUser>(u =>
            u.Email == googleUser.Email &&
            u.GoogleId == googleUser.Subject &&
            u.EmailConfirmed)), Times.Once);
        userManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidToken_DisabledUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var googleUser = ValidGoogleUser();
        var disabledUser = ActiveUser();
        disabledUser.IsActive = false;

        var userManager = BuildUserManagerMock();
        userManager.Setup(m => m.FindByEmailAsync(googleUser.Email))
            .ReturnsAsync(disabledUser);

        var validator = new Mock<IGoogleTokenValidator>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(googleUser);

        var handler = new GoogleLoginHandler(
            userManager.Object,
            new Mock<IJwtTokenService>().Object,
            validator.Object,
            JwtOptions());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new GoogleLoginCommand(new GoogleLoginRequest("id-token")), default).AsTask());
    }

    [Fact]
    public async Task Handle_InvalidToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange — validator simulates what GoogleTokenValidator does on bad tokens
        var validator = new Mock<IGoogleTokenValidator>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid or expired Google token."));

        var handler = new GoogleLoginHandler(
            BuildUserManagerMock().Object,
            new Mock<IJwtTokenService>().Object,
            validator.Object,
            JwtOptions());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new GoogleLoginCommand(new GoogleLoginRequest("bad-token")), default).AsTask());
    }

    [Fact]
    public async Task Handle_ValidToken_UserCreationFails_ThrowsBusinessException()
    {
        // Arrange
        var googleUser = ValidGoogleUser("fail@example.com");
        var identityErrors = new[] { new IdentityError { Description = "Duplicate email" } };

        var userManager = BuildUserManagerMock();
        userManager.Setup(m => m.FindByEmailAsync(googleUser.Email))
            .ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        var validator = new Mock<IGoogleTokenValidator>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(googleUser);

        var handler = new GoogleLoginHandler(
            userManager.Object,
            new Mock<IJwtTokenService>().Object,
            validator.Object,
            JwtOptions());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessException>(() =>
            handler.Handle(new GoogleLoginCommand(new GoogleLoginRequest("id-token")), default).AsTask());
        Assert.Contains("Duplicate email", ex.Errors);
    }

    [Fact]
    public async Task Handle_ValidToken_UnverifiedEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange — validator simulates what GoogleTokenValidator does when email_verified=false
        var validator = new Mock<IGoogleTokenValidator>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException(
                "Google account email address is not verified. Please verify your Google account email before signing in."));

        var handler = new GoogleLoginHandler(
            BuildUserManagerMock().Object,
            new Mock<IJwtTokenService>().Object,
            validator.Object,
            JwtOptions());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new GoogleLoginCommand(new GoogleLoginRequest("unverified-token")), default).AsTask());

        Assert.Contains("not verified", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}

// ── validator tests ────────────────────────────────────────────────────────────

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

// ── GoogleSettingsValidator tests ──────────────────────────────────────────────

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
