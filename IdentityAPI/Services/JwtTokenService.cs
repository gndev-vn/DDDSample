using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityAPI.Domain.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Authentication;
using Shared.Models;

namespace IdentityAPI.Services;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissions);
}

public class JwtTokenService(IOptions<JwtSettings> jwtSettings)
    : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly SigningCredentials _signingCredentials = new(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.SecretKey)),
        SecurityAlgorithms.HmacSha256);

    public Task<string> GenerateTokenAsync(ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var resolvedRoles = roles as string[] ?? roles.ToArray();
        var resolvedPermissions = permissions as string[] ?? permissions.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("firstName", user.FirstName),
            new("lastname", user.LastName)
        };

        if (user.CustomerId.HasValue)
        {
            claims.Add(new Claim(ApplicationClaimTypes.CustomerId, user.CustomerId.Value.ToString()));
        }

        claims.AddRange(resolvedRoles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(resolvedPermissions.Select(permission => new Claim(PermissionClaimTypes.Permission, permission)));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: _signingCredentials
        );

        return Task.FromResult(_tokenHandler.WriteToken(token));
    }
}
