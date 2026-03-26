using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityAPI.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Models;

namespace IdentityAPI.Services;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
    Task<string> GenerateTokenAsync(ApplicationUser user, IEnumerable<string> roles);
}

public class JwtTokenService(IOptions<JwtSettings> jwtSettings, UserManager<ApplicationUser> userManager)
    : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async Task<string> GenerateTokenAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return await GenerateTokenAsync(user, roles);
    }

    public Task<string> GenerateTokenAsync(ApplicationUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("firstName", user.FirstName),
            new("lastname", user.LastName)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: credentials
        );

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}
