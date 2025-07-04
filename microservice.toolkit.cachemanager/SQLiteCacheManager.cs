﻿using microservice.toolkit.cachemanager.serializer;
using microservice.toolkit.connection.extensions;
using microservice.toolkit.core;

using Microsoft.Data.Sqlite;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager;

/// <summary>
/// Before use SQLiteCacheManager, create a cache table:
/// <code>
/// CREATE TABLE cache(
///     id TEXT PRIMARY KEY,
///     value TEXT NOT NULL,
///     issuedAt INTEGER NOT NULL
/// );
/// </code>
/// </summary>
public class SQLiteCacheManager(SqliteConnection dbConnection, ICacheValueSerializer serializer)
    : ICacheManager
{
    private const string DeleteQuery = """
                                       DELETE FROM `cache`
                                       WHERE id = @id;         
                                       """;

    private const string UpsertQuery = """
                                       INSERT INTO `cache` (
                                           id,
                                           value, 
                                           issuedAt
                                       ) VALUES (
                                           @id, 
                                           @value,
                                           @issuedAt
                                       )
                                       ON CONFLICT(id) DO UPDATE SET 
                                           `value` = @value,
                                           issuedAt = @issuedAt;
                                       """;

    private const string SelectQuery = """
                                       SELECT value, issuedAt
                                       FROM cache 
                                       WHERE id = @CacheId AND ( issuedAt = 0 OR issuedAt >= @Now );         
                                       """;

    public SQLiteCacheManager(SqliteConnection dbConnection) : this(dbConnection,
        new JsonCacheValueSerializer())
    {
    }

    /// <summary>
    /// Retrieves the value associated with the specified key from the cache.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The value associated with the specified key, or the default value of <typeparamref name="TValue"/> if the key is not found in the cache.</returns>
    public async Task<TValue> GetAsync<TValue>(string key, CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, object>()
        {
            {"@CacheId", key}, {"@Now", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}
        };

        var value = await dbConnection.ExecuteScalarAsync<string>(SelectQuery, parameters);

        return value == null ? default : serializer.Deserialize<TValue>(value);
    }
    
    public bool TryGet<TValue>(string key, out TValue value)
    {
        value = this.Get<TValue>(key);
        return value != null;
    }

    public TValue Get<TValue>(string key)
    {
        var parameters = new Dictionary<string, object>()
        {
            {"@CacheId", key}, {"@Now", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}
        };

        var value = dbConnection.ExecuteScalar<string>(SelectQuery, parameters);

        return value == null ? default : serializer.Deserialize<TValue>(value);
    }

    /// <summary>
    /// With a "issuedAt" with a time in the past will result in the key being deleted rather than expired.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="issuedAt"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> SetAsync<TValue>(string key, TValue value, long issuedAt, CancellationToken cancellationToken)
    {
        if (issuedAt != 0 && issuedAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            await this.DeleteAsync(key, cancellationToken);
            return false;
        }

        var parameters = new Dictionary<string, object>()
        {
            {"@id", key}, {"@value", serializer.Serialize(value)}, {"@issuedAt", issuedAt}
        };

        return await dbConnection.ExecuteNonQueryAsync(UpsertQuery, parameters) != 0;
    }

    public bool Set<TValue>(string key, TValue value, long issuedAt)
    {
        if (issuedAt != 0 && issuedAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            this.Delete(key);
            return false;
        }

        var parameters = new Dictionary<string, object>()
        {
            {"@id", key}, {"@value", serializer.Serialize(value)}, {"@issuedAt", issuedAt}
        };

        return dbConnection.ExecuteNonQuery(UpsertQuery, parameters) != 0;
    }

    /// <summary>
    /// Sets the value associated with the specified key in the cache.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to be stored in the cache.</typeparam>
    /// <param name="key">The key of the value to be stored in the cache.</param>
    /// <param name="value">The value to be stored in the cache.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the operation was successful.</returns>
    public Task<bool> SetAsync<TValue>(string key, TValue value, CancellationToken cancellationToken)
    {
        return this.SetAsync(key, value, 0, cancellationToken);
    }
    
    public bool Set<TValue>(string key, TValue value)
    {
        return this.Set(key, value, 0);
    }

    /// <summary>
    /// Deletes a cache entry with the specified key.
    /// </summary>
    /// <param name="key">The key of the cache entry to delete.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation. The task result contains a boolean value indicating whether the cache entry was successfully deleted.</returns>
    public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, object> {{"@id", key}};

        return await dbConnection.ExecuteNonQueryAsync(DeleteQuery, parameters) != 0;
    }
    
    public bool Delete(string key)
    {
        var parameters = new Dictionary<string, object> {{"@id", key}};

        return dbConnection.ExecuteNonQuery(DeleteQuery, parameters) != 0;
    }
}