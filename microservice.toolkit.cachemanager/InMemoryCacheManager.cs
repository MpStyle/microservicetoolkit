using microservice.toolkit.cachemanager.serializer;
using microservice.toolkit.core;

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager;

public class InMemoryCacheManager : Disposable, ICacheManager
{
    private readonly ConcurrentDictionary<string, InMemoryItem> inMemory = new();
    private readonly ICacheValueSerializer serializer;

    public InMemoryCacheManager() : this(new JsonCacheValueSerializer())
    {
    }

    public InMemoryCacheManager(ICacheValueSerializer serializer)
    {
        this.serializer = serializer;
    }

    public Task<bool> Delete(string key)
    {
        return Task.FromResult(inMemory.TryRemove(key, out var _));
    }

    public Task<TValue> Get<TValue>(string key)
    {
        if (inMemory.TryGetValue(key, out var item) && item.IssuedAt > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            return Task.FromResult(this.serializer.Deserialize<TValue>(item.Value));
        }

        return Task.FromResult(default(TValue));
    }

    public async Task<bool> Set<TValue>(string key, TValue value, long issuedAt)
    {
        if (issuedAt != 0 && issuedAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            await this.Delete(key);
            return false;
        }

        var newItem = new InMemoryItem
        {
            Value = this.serializer.Serialize(value),
            IssuedAt = issuedAt
        };

        inMemory.AddOrUpdate(key, newItem, (key, oldValue) => newItem);

        return true;
    }

    public Task<bool> Set<TValue>(string key, TValue value)
    {
        var newItem = new InMemoryItem
        {
            Value = this.serializer.Serialize(value),
            IssuedAt = DateTimeOffset.UtcNow.AddYears(100).ToUnixTimeMilliseconds()
        };

        inMemory.AddOrUpdate(key, newItem, (key, oldValue) => newItem);

        return Task.FromResult(true); 
    }

    internal class InMemoryItem
    {
        public string Value { get; set; }
        public long IssuedAt { get; set; }
    }
}