using microservice.toolkit.cachemanager.serializer;
using microservice.toolkit.core;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using System;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager;

/// <summary>
/// Represents a cache manager that uses Redis as the underlying cache storage.
/// </summary>
public class RedisCacheManager : Disposable, ICacheManager
{
    private readonly ILogger<RedisCacheManager> logger;
    private readonly ConnectionMultiplexer connection;
    private readonly ICacheValueSerializer serializer;

    public RedisCacheManager(string connectionString, ILogger<RedisCacheManager> logger) : this(connectionString, new JsonCacheValueSerializer(), logger)
    {
    }

    public RedisCacheManager(string connectionString, ICacheValueSerializer serializer, ILogger<RedisCacheManager> logger)
    {
        this.serializer = serializer;
        this.connection = ConnectionMultiplexer.Connect(connectionString);
        this.logger = logger;
    }

    /// <summary>
    /// Deletes a cache entry with the specified key.
    /// </summary>
    /// <param name="key">The key of the cache entry to delete.</param>
    /// <returns>A task that represents the asynchronous delete operation. The task result contains a boolean value indicating whether the cache entry was successfully deleted.</returns>
    public Task<bool> DeleteAsync(string key)
    {
        var db = this.connection.GetDatabase();
        return db.KeyDeleteAsync(new RedisKey(key));
    }
    
    public bool Delete(string key)
    {
        var db = this.connection.GetDatabase();
        return db.KeyDelete(new RedisKey(key));
    }

    /// <summary>
    /// Retrieves the value associated with the specified key from the Redis cache.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The value associated with the specified key, or the default value of <typeparamref name="TValue"/> if the key is not found or the value is expired.</returns>
    /// <remarks>
    /// This method retrieves the value associated with the specified key from the Redis cache. If the value is found and not expired, it is deserialized and returned. If the value is expired or not found, the default value of <typeparamref name="TValue"/> is returned.
    /// </remarks>
    public async Task<TValue> GetAsync<TValue>(string key)
    {
        this.logger.LogDebug("Calling RedisCacheManager#Get({Empty})...", key ?? string.Empty);
        var db = this.connection.GetDatabase();
        var result = await db.StringGetWithExpiryAsync(key);

        if (result.Expiry.HasValue == false && result.Value.HasValue)
        {
            return this.serializer.Deserialize<TValue>(result.Value.ToString());
        }

        if (result.Expiry.HasValue && result.Expiry.Value.TotalMilliseconds > 0 && result.Value.HasValue)
        {
            return this.serializer.Deserialize<TValue>(result.Value.ToString());
        }

        return default;
    }
    
    public bool TryGet<TValue>(string key, out TValue value)
    {
        value = this.Get<TValue>(key);
        return value != null;
    }

    public TValue Get<TValue>(string key)
    {
        this.logger.LogDebug("Calling RedisCacheManager#Get({Empty})...", key ?? string.Empty);
        var db = this.connection.GetDatabase();
        var result = db.StringGetWithExpiry(key);

        if (result.Expiry.HasValue == false && result.Value.HasValue)
        {
            return this.serializer.Deserialize<TValue>(result.Value.ToString());
        }

        if (result.Expiry.HasValue && result.Expiry.Value.TotalMilliseconds > 0 && result.Value.HasValue)
        {
            return this.serializer.Deserialize<TValue>(result.Value.ToString());
        }

        return default;
    }

    /// <summary>
    /// Sets a value in the Redis cache with the specified key and expiration time.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to be stored in the cache.</typeparam>
    /// <param name="key">The key of the value to be stored in the cache.</param>
    /// <param name="value">The value to be stored in the cache.</param>
    /// <param name="issuedAt">The expiration time of the value in Unix timestamp format.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a boolean value indicating whether the value was successfully set in the cache.</returns>
    public async Task<bool> SetAsync<TValue>(string key, TValue value, long issuedAt)
    {
        if (issuedAt != 0 && issuedAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            await this.DeleteAsync(key);
            return false;
        }

        this.logger.LogDebug("Calling RedisCacheManager#Set({Empty})...", key ?? string.Empty);
        var db = this.connection.GetDatabase();
        var setResult = await db.StringSetAsync(key, this.serializer.Serialize(value), DateTimeOffset.FromUnixTimeMilliseconds(issuedAt).Subtract(DateTime.UtcNow));

        return setResult;
    }
    
    public bool Set<TValue>(string key, TValue value, long issuedAt)
    {
        if (issuedAt != 0 && issuedAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
             this.Delete(key);
            return false;
        }

        this.logger.LogDebug("Calling RedisCacheManager#Set({Empty})...", key ?? string.Empty);
        var db = this.connection.GetDatabase();
        var setResult = db.StringSet(key, this.serializer.Serialize(value), DateTimeOffset.FromUnixTimeMilliseconds(issuedAt).Subtract(DateTime.UtcNow));

        return setResult;
    }

    /// <summary>
    /// Sets a value in the Redis cache with the specified key.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to be stored in the cache.</typeparam>
    /// <param name="key">The key of the cache entry.</param>
    /// <param name="value">The value to be stored in the cache.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a boolean value indicating whether the operation was successful.</returns>
    public async Task<bool> SetAsync<TValue>(string key, TValue value)
    {
        this.logger.LogDebug("Calling RedisCacheManager#Set({Empty})...", key ?? string.Empty);
        var db = this.connection.GetDatabase();
        var setResult = await db.StringSetAsync(key, this.serializer.Serialize(value));
        return setResult;
    }
    
    public bool Set<TValue>(string key, TValue value)
    {
        this.logger.LogDebug("Calling RedisCacheManager#Set({Empty})...", key ?? string.Empty);
        var db = this.connection.GetDatabase();
        var setResult = db.StringSet(key, this.serializer.Serialize(value));
        return setResult;
    }
}
