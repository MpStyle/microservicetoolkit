using microservice.toolkit.core;

using Microsoft.Azure.Cosmos;

using Newtonsoft.Json;

using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager;

public class CosmosDbCacheManager(CosmosDbCacheManagerSettings settings) : Disposable, ICacheManager
{
    public bool Delete(string key)
    {
        return this.DeleteAsync(key).Result;
    }

    public async Task<bool> DeleteAsync(string key)
    {
        var item = await settings
            .Client
            .GetDatabase(settings.DatabaseId)
            .GetContainer(settings.ContainerId)
            .DeleteItemAsync<CosmosDbItem<object>>(key, new PartitionKey(settings.PartitionKey));
        return item.Resource != null;
    }

    public bool Set<TValue>(string key, TValue value, long issuedAt)
    {
        return this.SetAsync(key, value, issuedAt).Result;
    }

    public async Task<bool> SetAsync<TValue>(string key, TValue value, long issuedAt)
    {
        var item = await settings
            .Client
            .GetDatabase(settings.DatabaseId)
            .GetContainer(settings.ContainerId)
            .UpsertItemAsync<CosmosDbItem<TValue>>(new CosmosDbItem<TValue> {Id = key, Value = value, Ttl = issuedAt},
                new PartitionKey(settings.PartitionKey));

        return item.StatusCode == System.Net.HttpStatusCode.OK;
    }

    public bool Set<TValue>(string key, TValue value)
    {
        return this.SetAsync(key, value).Result;
    }

    public async Task<bool> SetAsync<TValue>(string key, TValue value)
    {
        var item = await settings
            .Client
            .GetDatabase(settings.DatabaseId)
            .GetContainer(settings.ContainerId)
            .UpsertItemAsync<CosmosDbItem<TValue>>(new CosmosDbItem<TValue> {Id = key, Value = value},
                new PartitionKey(settings.PartitionKey));

        return item.StatusCode == System.Net.HttpStatusCode.OK;
    }

    public TValue Get<TValue>(string key)
    {
        return this.GetAsync<TValue>(key).Result;
    }

    public async Task<TValue> GetAsync<TValue>(string key)
    {
        var item = await settings
            .Client
            .GetDatabase(settings.DatabaseId)
            .GetContainer(settings.ContainerId)
            .ReadItemAsync<CosmosDbItem<TValue>>(
                id: key,
                partitionKey: new PartitionKey(settings.PartitionKey)
            );

        return item.Resource.Value;
    }

    public bool TryGet<TValue>(string key, out TValue value)
    {
        var item = settings
            .Client
            .GetDatabase(settings.DatabaseId)
            .GetContainer(settings.ContainerId)
            .ReadItemAsync<CosmosDbItem<TValue>>(
                id: key,
                partitionKey: new PartitionKey(settings.PartitionKey)
            ).Result;

        if (item.StatusCode == System.Net.HttpStatusCode.OK)
        {
            value = item.Resource.Value;
            return true;
        }
        
        value = default;
        return false;
    }

    private record CosmosDbItem<TValue>
    {
        public string Id { get; set; }

        public TValue Value { get; set; }

        [JsonProperty("ttl")]
        [JsonPropertyName("ttl")]
        public long Ttl { get; set; }
    }
}

public record CosmosDbCacheManagerSettings
{
    public CosmosClient Client { get; set; }
    public string DatabaseId { get; set; }
    public string ContainerId { get; set; }
    public string PartitionKey { get; set; }
}