using microservice.toolkit.cachemanager.serializer;
using microservice.toolkit.core;

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager;

/// <summary>
/// Represents an in-memory cache manager that implements the <see cref="ICacheManager"/> interface.
/// </summary>
public class InMemoryCacheManager : Disposable, ICacheManager
{
    private readonly ConcurrentDictionary<string, InMemoryItem> inMemory = new();

    public InMemoryCacheManager()
    {
    }

    public Task<bool> Delete(string key)
    {
        return Task.FromResult(inMemory.TryRemove(key, out var _));
    }

    public Task<TValue> Get<TValue>(string key)
    {
        if (inMemory.TryGetValue(key, out var item)
            && item.IssuedAt > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            && item.Value is TValue typedValue)
        {
            return Task.FromResult(typedValue);
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
            Value = value,
            IssuedAt = issuedAt
        };

        inMemory.AddOrUpdate(key, newItem, (key, oldValue) => newItem);

        return true;
    }

    public Task<bool> Set<TValue>(string key, TValue value)
    {
        var newItem = new InMemoryItem
        {
            Value = value,
            IssuedAt = DateTimeOffset.UtcNow.AddYears(100).ToUnixTimeMilliseconds()
        };

        inMemory.AddOrUpdate(key, newItem, (key, oldValue) => newItem);

        return Task.FromResult(true);
    }

    internal class InMemoryItem
    {
        public object Value { get; set; }
        public long IssuedAt { get; set; }
    }
}