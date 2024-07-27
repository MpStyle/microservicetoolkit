using microservice.toolkit.cachemanager.serializer;
using microservice.toolkit.connectionmanager;
using microservice.toolkit.core;

using MySqlConnector;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager;

public class MysqlCacheManager : ICacheManager
{
    private readonly MySqlConnection connectionManager;
    private readonly ICacheValueSerializer serializer;

    private const string GetQuery = """
                                    SELECT value, issuedAt
                                    FROM cache 
                                    WHERE id = @CacheId AND ( issuedAt = 0 OR issuedAt >= @Now );         
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
                                       ON DUPLICATE KEY UPDATE 
                                           `value` = @value,
                                           issuedAt = @issuedAt
                                       """;

    private const string DeleteQuery = """
                                       DELETE FROM `cache`
                                       WHERE id = @id;
                                       """;

    public MysqlCacheManager(MySqlConnection connectionManager) : this(connectionManager,
        new JsonCacheValueSerializer())
    {
    }

    public MysqlCacheManager(MySqlConnection connectionManager, ICacheValueSerializer serializer)
    {
        this.serializer = serializer;
        this.connectionManager = connectionManager;
    }

    public async Task<TValue> GetAsync<TValue>(string key)
    {
        var parameters = new Dictionary<string, object>()
        {
            {"@CacheId", key}, {"@Now", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}
        };

        var value = await this.connectionManager.ExecuteScalarAsync<string>(GetQuery, parameters);

        return value == null ? default : this.serializer.Deserialize<TValue>(value);
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

        var value = this.connectionManager.ExecuteScalar<string>(GetQuery, parameters);

        return value == null ? default : this.serializer.Deserialize<TValue>(value);
    }

    /// <summary>
    /// With a "issuedAt" with a time in the past will result in the key being deleted rather than expired.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="issuedAt"></param>
    /// <returns></returns>
    public async Task<bool> SetAsync<TValue>(string key, TValue value, long issuedAt)
    {
        if (issuedAt != 0 && issuedAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            await this.DeleteAsync(key);
            return false;
        }

        var parameters = new Dictionary<string, object>()
        {
            {"@id", key}, {"@value", this.serializer.Serialize(value)}, {"@issuedAt", issuedAt}
        };

        return await this.connectionManager.ExecuteNonQueryAsync(UpsertQuery, parameters) != 0;
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
            {"@id", key}, {"@value", this.serializer.Serialize(value)}, {"@issuedAt", issuedAt}
        };

        return this.connectionManager.ExecuteNonQuery(UpsertQuery, parameters) != 0;   
    }

    public Task<bool> SetAsync<TValue>(string key, TValue value)
    {
        return this.SetAsync(key, value, 0);
    }
    
    public bool Set<TValue>(string key, TValue value)
    {
        return this.Set(key, value, 0);
    }

    public async Task<bool> DeleteAsync(string key)
    {
        var parameters = new Dictionary<string, object> {{"@id", key},};

        return await this.connectionManager.ExecuteNonQueryAsync(DeleteQuery, parameters) != 0;
    }
    
    public bool Delete(string key)
    {
        var parameters = new Dictionary<string, object> {{"@id", key},};

        return this.connectionManager.ExecuteNonQuery(DeleteQuery, parameters) != 0;
    }
}