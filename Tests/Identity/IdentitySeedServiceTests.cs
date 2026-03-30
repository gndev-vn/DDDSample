using IdentityAPI.Configuration;
using IdentityAPI.Domain.Identity;
using IdentityAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DDDSample.Tests.Identity;

public sealed class IdentitySeedServiceTests
{
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

    private static Mock<RoleManager<ApplicationRole>> BuildRoleManagerMock()
    {
        var store = new Mock<IRoleStore<ApplicationRole>>();
        return new Mock<RoleManager<ApplicationRole>>(
            store.Object,
            Array.Empty<IRoleValidator<ApplicationRole>>(),
            new Mock<ILookupNormalizer>().Object,
            new IdentityErrorDescriber(),
            new Mock<ILogger<RoleManager<ApplicationRole>>>().Object);
    }

    private static IdentitySeedOptions CreateOptions() => new()
    {
        Enabled = true,
        Admin = new SeedUserOptions
        {
            Username = "admin",
            Email = "admin@example.com",
            Password = "admin123",
            FirstName = "Admin",
            LastName = "User"
        },
        User = new SeedUserOptions
        {
            Username = "user",
            Email = "user@example.com",
            Password = "user123",
            FirstName = "Sample",
            LastName = "User"
        }
    };

    [Fact]
    public async Task SeedAsync_CreatesMissingRolesAndUsers()
    {
        var userManager = BuildUserManagerMock();
        var roleManager = BuildRoleManagerMock();

        roleManager.Setup(m => m.RoleExistsAsync(IdentityRoleNames.Admin)).ReturnsAsync(false);
        roleManager.Setup(m => m.RoleExistsAsync(IdentityRoleNames.User)).ReturnsAsync(false);
        roleManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationRole>())).ReturnsAsync(IdentityResult.Success);

        userManager.Setup(m => m.FindByEmailAsync("admin@example.com")).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(m => m.FindByEmailAsync("user@example.com")).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync([]);
        userManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        var service = new IdentitySeedService(
            userManager.Object,
            roleManager.Object,
            Options.Create(CreateOptions()),
            Mock.Of<ILogger<IdentitySeedService>>());

        await service.SeedAsync();

        roleManager.Verify(m => m.CreateAsync(It.Is<ApplicationRole>(role => role.Name == IdentityRoleNames.Admin)), Times.Once);
        roleManager.Verify(m => m.CreateAsync(It.Is<ApplicationRole>(role => role.Name == IdentityRoleNames.User)), Times.Once);
        userManager.Verify(m => m.CreateAsync(It.Is<ApplicationUser>(user =>
            user.UserName == "admin" && user.Email == "admin@example.com"), "admin123"), Times.Once);
        userManager.Verify(m => m.CreateAsync(It.Is<ApplicationUser>(user =>
            user.UserName == "user" && user.Email == "user@example.com"), "user123"), Times.Once);
        userManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), IdentityRoleNames.Admin), Times.Once);
        userManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), IdentityRoleNames.User), Times.Once);
    }

    [Fact]
    public async Task SeedAsync_DoesNotRecreateExistingRolesOrUsers_ButAddsMissingRoleAssignment()
    {
        var adminUser = ApplicationUser.CreateLocal("admin", "admin@example.com", "Admin", "User", DateTime.UtcNow);
        var normalUser = ApplicationUser.CreateLocal("user", "user@example.com", "Sample", "User", DateTime.UtcNow);

        var userManager = BuildUserManagerMock();
        var roleManager = BuildRoleManagerMock();

        roleManager.Setup(m => m.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        userManager.Setup(m => m.FindByEmailAsync("admin@example.com")).ReturnsAsync(adminUser);
        userManager.Setup(m => m.FindByEmailAsync("user@example.com")).ReturnsAsync(normalUser);
        userManager.Setup(m => m.GetRolesAsync(adminUser)).ReturnsAsync([IdentityRoleNames.Admin]);
        userManager.Setup(m => m.GetRolesAsync(normalUser)).ReturnsAsync([]);
        userManager.Setup(m => m.AddToRoleAsync(normalUser, IdentityRoleNames.User)).ReturnsAsync(IdentityResult.Success);

        var service = new IdentitySeedService(
            userManager.Object,
            roleManager.Object,
            Options.Create(CreateOptions()),
            Mock.Of<ILogger<IdentitySeedService>>());

        await service.SeedAsync();

        roleManager.Verify(m => m.CreateAsync(It.IsAny<ApplicationRole>()), Times.Never);
        userManager.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        userManager.Verify(m => m.AddToRoleAsync(adminUser, IdentityRoleNames.Admin), Times.Never);
        userManager.Verify(m => m.AddToRoleAsync(normalUser, IdentityRoleNames.User), Times.Once);
    }
}
