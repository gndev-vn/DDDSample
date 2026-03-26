using System.Collections.Concurrent;
using IdentityTokenBlacklistService = IdentityAPI.Services.TokenBlacklistService;
using Microsoft.Extensions.Caching.Distributed;
using SharedTokenBlacklistService = Shared.Services.TokenBlacklistService;

namespace DDDSample.Tests.Identity;

public sealed class TokenBlacklistServiceTests
{
    [Fact]
    public async Task IdentityTokenBlacklistService_UsesConsistentKeyForRevocation()
    {
        var cache = new InMemoryDistributedCache();
        var service = new IdentityTokenBlacklistService(cache);
        const string token = "header.payload.signature";

        await service.RevokeTokenAsync(token, TimeSpan.FromMinutes(15));

        Assert.True(await service.IsTokenRevokedAsync(token));
    }

    [Fact]
    public async Task SharedTokenBlacklistService_UsesConsistentKeyForRevocation()
    {
        var cache = new InMemoryDistributedCache();
        var service = new SharedTokenBlacklistService(cache);
        const string token = "header.payload.signature";

        await service.RevokeTokenAsync(token, TimeSpan.FromMinutes(15));

        Assert.True(await service.IsTokenRevokedAsync(token));
    }

    private sealed class InMemoryDistributedCache : IDistributedCache
    {
        private readonly ConcurrentDictionary<string, byte[]> _store = new();

        public byte[]? Get(string key) => _store.TryGetValue(key, out var value) ? value : null;

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
            => Task.FromResult(Get(key));

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
            => Task.CompletedTask;

        public void Remove(string key)
        {
            _store.TryRemove(key, out _);
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            _store[key] = value;
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }
    }
}
