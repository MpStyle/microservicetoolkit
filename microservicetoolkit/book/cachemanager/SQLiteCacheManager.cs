using mpstyle.microservice.toolkit.book.connectionmanager;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.cachemanager
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
        private readonly SQLiteConnectionManager connectionManager;

        public SQLiteCacheManager(SQLiteConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;
        }

        public async Task<string> Get(string key)
        {
            var parameters = new Dictionary<string, object>(){
                {"@CacheId", key}
            };

            var query = @"
                SELECT value, issuedAt
                FROM cache 
                WHERE id = @CacheId;
            ";

            return await this.connectionManager.ExecuteAsync(async (DbCommand cmd) =>
            {
                cmd.CommandText = query;
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.Add(this.connectionManager.GetParameter(parameter.Key, parameter.Value));
                }

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        var value = reader.GetString(0);
                        var issuedAt = reader.GetInt64(1);

                        if (issuedAt == 0 || issuedAt >= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                        {
                            return value;
                        }
                    }

                    return default;
                }
            });


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
                ON CONFLICT(id) DO UPDATE SET 
                    `value` = @value,
                    issuedAt = @issuedAt;
            ";

            var parameters = new Dictionary<string, object>(){
                {"@id", key},
                {"@value", value},
                {"@issuedAt", issuedAt}
            };

            return await this.connectionManager.ExecuteAsync(async (DbCommand cmd) =>
            {
                cmd.CommandText = query;
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.Add(this.connectionManager.GetParameter(parameter.Key, parameter.Value));
                }

                return await cmd.ExecuteNonQueryAsync() != 0;
            });
        }

        public Task<bool> Set(string key, string value)
        {
            return this.Set(key, value, 0);
        }

        public async Task<bool> Delete(string key)
        {
            var query = @"
                DELETE FROM `cache`
                WHERE id = @id;
            ";

            var parameters = new Dictionary<string, object>(){
                {"@id", key},
            };

            return await this.connectionManager.ExecuteAsync(async (DbCommand cmd) =>
            {
                cmd.CommandText = query;
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.Add(this.connectionManager.GetParameter(parameter.Key, parameter.Value));
                }

                return await cmd.ExecuteNonQueryAsync() != 0;
            });
        }
    }
}