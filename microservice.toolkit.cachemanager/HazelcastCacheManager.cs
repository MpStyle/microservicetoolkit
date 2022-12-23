using Hazelcast;

using microservice.toolkit.cachemanager.serializer;
using microservice.toolkit.core;

using System;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager;

public class HazelcastCacheManager : Disposable, ICacheManager
{
    private readonly ICacheValueSerializer serializer;
    private readonly HazelcastOptions options;
    private readonly string distributedMap;

    public HazelcastCacheManager(HazelcastOptions options, string distributedMap) : this(options, distributedMap, new JsonCacheValueSerializer())
    {
    }

    public HazelcastCacheManager(HazelcastOptions options, string distributedMap, ICacheValueSerializer serializer)
    {
        this.options = options;
        this.serializer = serializer;
        this.distributedMap = distributedMap;
    }

    public async Task<bool> Delete(string key)
    {
        await using var client = await HazelcastClientFactory.StartNewClientAsync(this.options);
        await using var map = await client.GetMapAsync<string, string>(this.distributedMap);
        await map.DeleteAsync(key);
        return true;
    }

    public async Task<TValue> Get<TValue>(string key)
    {
        await using var client = await HazelcastClientFactory.StartNewClientAsync(this.options);
        await using var map = await client.GetMapAsync<string, string>(this.distributedMap);
        var value = await map.GetAsync(key);
        return this.serializer.Deserialize<TValue>(value);
    }

    public async Task<bool> Set<TValue>(string key, TValue value, long issuedAt)
    {
        await using var client = await HazelcastClientFactory.StartNewClientAsync(this.options);
        await using var map = await client.GetMapAsync<string, string>(this.distributedMap);
        await map.SetAsync(key, this.serializer.Serialize(value), DateTimeOffset.FromUnixTimeMilliseconds(issuedAt).Subtract(DateTime.UtcNow));
        return true;
    }

    public async Task<bool> Set<TValue>(string key, TValue value)
    {
        await using var client = await HazelcastClientFactory.StartNewClientAsync(this.options);
        await using var map = await client.GetMapAsync<string, string>(this.distributedMap);
        await map.SetAsync(key, this.serializer.Serialize(value));
        return true;
    }
}