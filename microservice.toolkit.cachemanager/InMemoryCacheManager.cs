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
        if (this.inMemory.Count >= settings.Capacity)
        {
            var issuedItems = this.inMemory
                .Where(item => item.Value.IssuedAt <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                .ToList();
            foreach (var issuedItem in issuedItems)
            {
                await this.Delete(issuedItem.Key);
            }

            if (issuedItems.Count == 0)
            {
                var oldestItem = this.inMemory
                    .OrderBy(item => item.Value.InsertedAt)
                    .First();
                await this.Delete(oldestItem.Key);
            }
        }

        if (issuedAt != 0 && issuedAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            await this.Delete(key);
            return false;
        }

        var newItem = new InMemoryItem {Value = value, IssuedAt = issuedAt};

        inMemory.AddOrUpdate(key, newItem, (key, oldValue) => newItem);

        return true;
    }

    public Task<bool> Set<TValue>(string key, TValue value)
    {
        return this.Set(key, value, DateTimeOffset.UtcNow.AddYears(100).ToUnixTimeMilliseconds());
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