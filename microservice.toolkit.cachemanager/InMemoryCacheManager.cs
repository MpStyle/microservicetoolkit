using microservice.toolkit.core;

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager;

public class InMemoryCacheManager : Disposable, ICacheManager
{
    private readonly ConcurrentDictionary<string, InMemoryItem> inMemory = new();

    public Task<bool> Delete(string key)
    {
        return Task.FromResult(inMemory.TryRemove(key, out var _));
    }

    public Task<string> Get(string key)
    {
        if(inMemory.TryGetValue(key, out var item) && item.IssuedAt > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) {
            return Task.FromResult(item.Value);
        }

        return Task.FromResult(default(string));
    }

    public async Task<bool> Set(string key, string value, long issuedAt)
    {
        if (issuedAt != 0 && issuedAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            await this.Delete(key);
            return false;
        }

        var newItem = new InMemoryItem {
            Value=value,
            IssuedAt=issuedAt
        };

        inMemory.AddOrUpdate(key, newItem, (key, oldValue) => newItem);

        return true;
    }

    public Task<bool> Set(string key, string value)
    {
        var newItem = new InMemoryItem
        {
            Value = value,
            IssuedAt = DateTimeOffset.UtcNow.AddYears(100).ToUnixTimeMilliseconds()
        };

        inMemory.AddOrUpdate(key, newItem, (key, oldValue) => newItem);

        return Task.FromResult(true); ;
    }

    internal class InMemoryItem {
        public string Value { get; set; }
        public long IssuedAt { get; set; }
    }
}