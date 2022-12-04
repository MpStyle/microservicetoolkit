
using Enyim.Caching.Memcached;

using microservice.toolkit.cachemanager.serializer;
using microservice.toolkit.core;

using System;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager
{
    public class MemcachedCacheManager : Disposable, ICacheManager
    {
        private readonly MemcachedCluster cluster;
        private readonly IMemcachedClient client;

        public MemcachedCacheManager(string servers)
        {
            this.cluster = new MemcachedCluster(servers);
            this.cluster.Start();
            this.client = cluster.GetClient();
        }

        public Task<bool> Delete(string key)
        {
            return this.client.DeleteAsync(key);
        }

        public async Task<TValue> Get<TValue>(string key)
        {
            return await this.client.GetAsync<TValue>(key);
        }

        public Task<bool> Set<TValue>(string key, TValue value, long issuedAt)
        {
            if (issuedAt != 0 && issuedAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            {
                this.Delete(key);
                return Task.FromResult(false);
            }

            var duration = (uint)((issuedAt - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) / 1000);

            return this.client.SetAsync(key, value, new Expiration(duration));
        }

        public Task<bool> Set<TValue>(string key, TValue value)
        {
            return this.client.SetAsync(key, value);
        }

        protected override void DisposeManage()
        {
            base.DisposeManage();
            this.cluster.Dispose();
        }
    }
}