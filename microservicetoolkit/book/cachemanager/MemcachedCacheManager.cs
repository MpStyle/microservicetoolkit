
using Enyim.Caching.Memcached;

using System.Threading.Tasks;

namespace mpstyle.microservice.toolkit.book.cachemanager
{
    public class MemcachedCacheManager : Disposable, ICacheManager
    {
        private readonly MemcachedCluster cluster;
        private readonly IMemcachedClient client;

        public MemcachedCacheManager(IConfigurationManager configurationManager)
        {
            this.cluster = new MemcachedCluster(configurationManager.GetString(SettingKey.Cache.SERVERS));
            this.cluster.Start();
            this.client = cluster.GetClient();
        }

        public async Task<string> Get(string key)
        {
            var response = await this.client.GetAsync(key);
            return response as string;
        }

        public Task<bool> Set(string key, string value, long issuedAt)
        {
            return this.client.SetAsync(key, value, new Expiration((uint)issuedAt));
        }

        protected override void DisposeManage()
        {
            base.DisposeManage();
            this.cluster.Dispose();
        }
    }
}