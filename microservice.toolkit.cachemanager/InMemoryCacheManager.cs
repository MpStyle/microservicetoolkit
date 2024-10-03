using microservice.toolkit.core;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager;

/// <summary>
/// Represents an in-memory cache manager that implements the <see cref="ICacheManager"/> interface.
/// </summary>
public class InMemoryCacheManager(InMemoryCacheManagerSettings settings) : Disposable, ICacheManager
{
    private readonly ConcurrentDictionary<string, InMemoryItem> inMemory = new();

    public InMemoryCacheManager() : this(new InMemoryCacheManagerSettings())
    {
    }
    
    public bool Delete(string key)
    {
        return inMemory.TryRemove(key, out var _);
    }

    public Task<bool> DeleteAsync(string key)
    {
        return Task.FromResult(this.Delete(key));
    }

    public TValue Get<TValue>(string key)
    {
        if (inMemory.TryGetValue(key, out var item)
            && item.IssuedAt > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            && item.Value is TValue typedValue)
        {
            return typedValue;
        }

        return default;
    }
    
    public Task<TValue> GetAsync<TValue>(string key)
    {
        return Task.FromResult(this.Get<TValue>(key));
    }

    public bool TryGet<TValue>(string key, out TValue value)
    {
        value = this.Get<TValue>(key);
        return value != null;
    }

    public bool Set<TValue>(string key, TValue value, long issuedAt)
    {
        if (this.inMemory.Count >= settings.Capacity)
        {
            var issuedItems = this.inMemory
                .Where(item => item.Value.IssuedAt <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                .ToList();
            foreach (var issuedItem in issuedItems)
            {
                this.Delete(issuedItem.Key);
            }

            if (issuedItems.Count == 0)
            {
                var oldestItem = this.inMemory
                    .OrderBy(item => item.Value.InsertedAt)
                    .First();
                this.Delete(oldestItem.Key);
            }
        }

        if (issuedAt != 0 && issuedAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            this.Delete(key);
            return false;
        }

        var newItem = new InMemoryItem {Value = value, IssuedAt = issuedAt};

        inMemory.AddOrUpdate(key, newItem, (_, _) => newItem);

        return true;
    }

    public Task<bool> SetAsync<TValue>(string key, TValue value, long issuedAt)
    {
        return Task.FromResult(this.Set(key, value, issuedAt));
    }

    public bool Set<TValue>(string key, TValue value)
    {
        return this.Set(key, value, DateTimeOffset.UtcNow.AddYears(100).ToUnixTimeMilliseconds());
    }

    public Task<bool> SetAsync<TValue>(string key, TValue value)
    {
        return Task.FromResult(this.Set(key, value));
    }

    private record InMemoryItem
    {
        public object Value { get; set; }
        public long IssuedAt { get; set; }
        public long InsertedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}

public record InMemoryCacheManagerSettings
{
    public int Capacity { get; set; } = 100;
}