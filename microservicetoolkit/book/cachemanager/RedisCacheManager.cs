using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.cachemanager
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

        public async Task<string> Get(string key)
        {
            this.logger.LogDebug($"Calling RedisCacheManager#Get({key ?? string.Empty})...");
            var db = this.connection.GetDatabase();
            var result = await db.StringGetWithExpiryAsync(key);

            if (result.Expiry.HasValue && result.Expiry.Value.TotalMilliseconds > 0 && result.Value.HasValue)
            {
                return result.Value.ToString();
            }

            return null;
        }

        public async Task<bool> Set(string key, string value, long issuedAt)
        {
            this.logger.LogDebug($"Calling RedisCacheManager#Set({key ?? string.Empty})...");
            var db = this.connection.GetDatabase();
            var setResult = await db.StringSetAsync(key, value);

            if (setResult)
            {
                return await db.KeyExpireAsync(key, DateTimeUtils.UnixTimeStampToDateTime(issuedAt));
            }

            return setResult;
        }
    }
}
