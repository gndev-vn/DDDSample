using Microsoft.Extensions.Caching.Distributed;

namespace Shared.Services;

public interface ITokenBlacklistService
{
    Task RevokeTokenAsync(string token, TimeSpan expiresIn);
    Task<bool> IsTokenRevokedAsync(string token);
}

public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly IDistributedCache _cache;
    private const string BlacklistPrefix = "blacklist";

    public TokenBlacklistService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task RevokeTokenAsync(string token, TimeSpan expiresIn)
    {
        var key = $"{BlacklistPrefix}:{token}";
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiresIn
        };
        
        await _cache.SetStringAsync(key, "revoked", options);
    }

    public async Task<bool> IsTokenRevokedAsync(string token)
    {
        var key = $"{BlacklistPrefix}:{token}";
        var value = await _cache.GetStringAsync(key);
        return value != null;
    }
}