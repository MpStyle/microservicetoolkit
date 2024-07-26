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
    public Task<bool> DeleteAsync(string key)
    {
        return this.client.RemoveAsync(key);
    }
    
    public bool Delete(string key)
    {
        return this.client.Remove(key);
    }

    public async Task<TValue> GetAsync<TValue>(string key)
    {
        var result = await this.client.GetAsync<TValue>(key);

        return result.HasValue ? result.Value : default;
    }
    
    public TValue Get<TValue>(string key)
    {
        return this.client.Get<TValue>(key);
    }

    /// <inheritdoc/>
    public bool TryGet<TValue>(string key, out TValue value)
    {
        value = this.client.Get<TValue>(key);

        return value != null;
    }

    /// <inheritdoc/>
    public async Task<bool> SetAsync<TValue>(string key, TValue value, long issuedAt)
    {
        if (issuedAt != 0 && issuedAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            await this.DeleteAsync(key);
            return false;
        }

        var duration = (int)((issuedAt - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) / 1000);

        return await this.client.SetAsync(key, value, duration);
    }
    
    public bool Set<TValue>(string key, TValue value, long issuedAt)
    {
        if (issuedAt != 0 && issuedAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            this.Delete(key);
            return false;
        }

        var duration = (int)((issuedAt - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) / 1000);

        return this.client.Set(key, value, duration);
    }

    /// <inheritdoc/>
    public Task<bool> SetAsync<TValue>(string key, TValue value)
    {
        return this.SetAsync(key, value, DateTimeOffset.UtcNow.AddYears(100).ToUnixTimeMilliseconds());
    }
    
    public bool Set<TValue>(string key, TValue value)
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