using Enyim.Caching;

using microservice.toolkit.core;

using System;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager;

/// <summary>
/// Represents a cache manager that uses Memcached as the underlying caching mechanism.
/// </summary>
public class MemcachedCacheManager : Disposable, ICacheManager
{
    private readonly IMemcachedClient client;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemcachedCacheManager"/> class.
    /// </summary>
    /// <param name="client">The Memcached client used for caching operations.</param>
    public MemcachedCacheManager(IMemcachedClient client)
    {
        this.client = client;
    }

    /// <inheritdoc/>
    public Task<bool> Delete(string key)
    {
        return this.client.RemoveAsync(key);
    }

    /// <inheritdoc/>
    public async Task<TValue> Get<TValue>(string key)
    {
        var result = await this.client.GetAsync<TValue>(key);

        return result.HasValue ? result.Value : default;
    }

    /// <inheritdoc/>
    public Task<bool> Set<TValue>(string key, TValue value, long issuedAt)
    {
        if (issuedAt != 0 && issuedAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            this.Delete(key);
            return Task.FromResult(false);
        }

        var duration = (int)((issuedAt - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) / 1000);

        return this.client.SetAsync(key, value, duration);
    }

    /// <inheritdoc/>
    public Task<bool> Set<TValue>(string key, TValue value)
    {
        return this.Set(key, value, DateTimeOffset.UtcNow.AddYears(100).ToUnixTimeMilliseconds());
    }

    /// <inheritdoc/>
    protected override void DisposeManage()
    {
        base.DisposeManage();
        this.client.Dispose();
    }
}