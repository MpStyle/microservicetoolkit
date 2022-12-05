using microservice.toolkit.cachemanager.serializer;
using microservice.toolkit.connectionmanager;
using microservice.toolkit.core;

using Microsoft.Data.Sqlite;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager
{
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
    public class SQLiteCacheManager : ICacheManager
    {
        private readonly SqliteConnection connectionManager;
        private readonly ICacheValueSerializer serializer;

        public SQLiteCacheManager(SqliteConnection connectionManager) : this(connectionManager, new JsonCacheValueSerializer())
        {
        }

        public SQLiteCacheManager(SqliteConnection connectionManager, ICacheValueSerializer serializer)
        {
            this.serializer = serializer;
            this.connectionManager = connectionManager;
        }

        public async Task<TValue> Get<TValue>(string key)
        {
            var parameters = new Dictionary<string, object>(){
                {"@CacheId", key},
                {"@Now", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
            };

            const string query = @"
                SELECT value, issuedAt
                FROM cache 
                WHERE id = @CacheId AND ( issuedAt = 0 OR issuedAt >= @Now );
            ";

            var value = await this.connectionManager.ExecuteFirstAsync(query, reader => reader.GetString(0), parameters);

            if (value == null)
            {
                return default;
            }

            return this.serializer.Deserialize<TValue>(value);
        }

        /// <summary>
        /// With a "issuedAt" with a time in the past will result in the key being deleted rather than expired.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="issuedAt"></param>
        /// <returns></returns>
        public async Task<bool> Set<TValue>(string key, TValue value, long issuedAt)
        {
            if (issuedAt != 0 && issuedAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            {
                await this.Delete(key);
                return false;
            }

            const string query = @"
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
            ";

            var parameters = new Dictionary<string, object>(){
                {"@id", key},
                {"@value", this.serializer.Serialize(value)},
                {"@issuedAt", issuedAt}
            };

            return await this.connectionManager.ExecuteNonQueryAsync(query, parameters) != 0;
        }

        public Task<bool> Set<TValue>(string key, TValue value)
        {
            return this.Set(key, value, 0);
        }

        public async Task<bool> Delete(string key)
        {
            const string query = @"
                DELETE FROM `cache`
                WHERE id = @id;
            ";

            var parameters = new Dictionary<string, object>(){
                {"@id", key},
            };

            return await this.connectionManager.ExecuteNonQueryAsync(query, parameters) != 0;
        }
    }
}