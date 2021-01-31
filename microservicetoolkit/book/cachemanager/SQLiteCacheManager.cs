using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.cachemanager
{
    public class SQLiteCacheManager : ICacheManager
    {
        private readonly IConnectionManager connectionManager;

        public SQLiteCacheManager(IConnectionManager connectionManager)
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

                        if (issuedAt == 0 || issuedAt >= DateTime.Now.ToUniversalTime().ToEpoch())
                        {
                            return value;
                        }
                    }

                    return null;
                }
            });


        }

        public async Task<bool> Set(string key, string value, long issuedAt)
        {
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
    }
}