using microservice.toolkit.connectionmanager;
using microservice.toolkit.core;

using MySqlConnector;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager
{
    public class MysqlCacheManager : ICacheManager
    {
        private readonly MySqlConnection connectionManager;

        public MysqlCacheManager(MySqlConnection connectionManager)
        {
            this.connectionManager = connectionManager;
        }

        public async Task<string> Get(string key)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "@CacheId", key }, { "@Now", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
            };

            const string query = @"
                SELECT value, issuedAt
                FROM cache 
                WHERE id = @CacheId AND ( issuedAt = 0 OR issuedAt >= @Now );
            ";

            return await this.connectionManager.ExecuteFirstAsync(query, reader => reader.GetString(0), parameters);
        }

        /// <summary>
        /// With a "issuedAt" with a time in the past will result in the key being deleted rather than expired.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="issuedAt"></param>
        /// <returns></returns>
        public async Task<bool> Set(string key, string value, long issuedAt)
        {
            if (issuedAt != 0 && issuedAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            {
                await this.Delete(key);
                return false;
            }

            var query = @"
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
            ";

            var parameters =
                new Dictionary<string, object>() { { "@id", key }, { "@value", value }, { "@issuedAt", issuedAt } };

            return await this.connectionManager.ExecuteNonQueryAsync(query, parameters) != 0;
        }

        public Task<bool> Set(string key, string value)
        {
            return this.Set(key, value, 0);
        }

        public async Task<bool> Delete(string key)
        {
            const string query = @"
                DELETE FROM `cache`
                WHERE id = @id;
            ";

            var parameters = new Dictionary<string, object>() { { "@id", key }, };

            return await this.connectionManager.ExecuteNonQueryAsync(query, parameters) != 0;
        }
    }
}