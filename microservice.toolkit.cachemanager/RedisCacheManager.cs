using microservice.toolkit.core;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using System;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager
{
    public class RedisCacheManager : Disposable, ICacheManager
    {
        private readonly ILogger<RedisCacheManager> logger;
        private readonly ConnectionMultiplexer connection;

        public RedisCacheManager(string connectionString, ILogger<RedisCacheManager> logger)
        {
            this.connection = ConnectionMultiplexer.Connect(connectionString);
            this.logger = logger;
        }

        public Task<bool> Delete(string key)
        {
            var db = this.connection.GetDatabase();
            return db.KeyDeleteAsync(new RedisKey(key));
        }

        public async Task<string> Get(string key)
        {
            this.logger.LogDebug($"Calling RedisCacheManager#Get({key ?? string.Empty})...");
            var db = this.connection.GetDatabase();
            var result = await db.StringGetWithExpiryAsync(key);

            if (result.Expiry.HasValue == false && result.Value.HasValue)
            {
                return result.Value.ToString();
            }

            if (result.Expiry.HasValue && result.Expiry.Value.TotalMilliseconds > 0 && result.Value.HasValue)
            {
                return result.Value.ToString();
            }

            return null;
        }

        public async Task<bool> Set(string key, string value, long issuedAt)
        {
            if (issuedAt != 0 && issuedAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            {
                await this.Delete(key);
                return false;
            }

            this.logger.LogDebug($"Calling RedisCacheManager#Set({key ?? string.Empty})...");
            var db = this.connection.GetDatabase();
            var setResult = await db.StringSetAsync(key, value, DateTimeOffset.FromUnixTimeMilliseconds(issuedAt).Subtract(DateTime.UtcNow));

            return setResult;
        }

        public async Task<bool> Set(string key, string value)
        {
            this.logger.LogDebug($"Calling RedisCacheManager#Set({key ?? string.Empty})...");
            var db = this.connection.GetDatabase();
            var setResult = await db.StringSetAsync(key, value);
            return setResult;
        }
    }
}
