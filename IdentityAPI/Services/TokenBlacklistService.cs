using Microsoft.Extensions.Caching.Distributed;

namespace IdentityAPI.Services;

public interface ITokenBlacklistService
{
    Task RevokeTokenAsync(string token, TimeSpan expiresIn);
    Task<bool> IsTokenRevokedAsync(string token);
}

public class TokenBlacklistService(IDistributedCache cache) : ITokenBlacklistService
{
    private const string BlacklistPrefix = "blacklist";

    public async Task RevokeTokenAsync(string token, TimeSpan expiresIn)
    {
        var key = $"{BlacklistPrefix}:{token}";
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiresIn
        };
        
        await cache.SetStringAsync(key, "revoked", options);
    }

    public async Task<bool> IsTokenRevokedAsync(string token)
    {
        var key = $"{BlacklistPrefix}{token}";
        var value = await cache.GetStringAsync(key);
        return value != null;
    }
}