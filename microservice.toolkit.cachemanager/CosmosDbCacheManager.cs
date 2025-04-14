using microservice.toolkit.core;

using Microsoft.Azure.Cosmos;

using Newtonsoft.Json;

using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace microservice.toolkit.cachemanager;

public class CosmosDbCacheManager(CosmosDbCacheManagerSettings settings) : Disposable, ICacheManager
{
    public bool Delete(string key)
    {
        return this.DeleteAsync(key, CancellationToken.None).Result;
    }

    public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken)
    {
        var item = await settings
            .Client
            .GetDatabase(settings.DatabaseId)
            .GetContainer(settings.ContainerId)
            .DeleteItemAsync<CosmosDbItem<object>>(key, new PartitionKey(settings.PartitionKey), cancellationToken: cancellationToken);
        return item.Resource != null;
    }

    public bool Set<TValue>(string key, TValue value, long issuedAt)
    {
        return this.SetAsync(key, value, issuedAt, CancellationToken.None).Result;
    }

    public async Task<bool> SetAsync<TValue>(string key, TValue value, long issuedAt, CancellationToken cancellationToken)
    {
        var item = await settings
            .Client
            .GetDatabase(settings.DatabaseId)
            .GetContainer(settings.ContainerId)
            .UpsertItemAsync(new CosmosDbItem<TValue> {Id = key, Value = value, Ttl = issuedAt},
                new PartitionKey(settings.PartitionKey), cancellationToken: cancellationToken);

        return item.StatusCode == System.Net.HttpStatusCode.OK;
    }

    public bool Set<TValue>(string key, TValue value)
    {
        return this.SetAsync(key, value, CancellationToken.None).Result;
    }

    public async Task<bool> SetAsync<TValue>(string key, TValue value, CancellationToken cancellationToken)
    {
        var item = await settings
            .Client
            .GetDatabase(settings.DatabaseId)
            .GetContainer(settings.ContainerId)
            .UpsertItemAsync(new CosmosDbItem<TValue> {Id = key, Value = value},
                new PartitionKey(settings.PartitionKey), cancellationToken: cancellationToken);

        return item.StatusCode == System.Net.HttpStatusCode.OK;
    }

    public TValue Get<TValue>(string key)
    {
        return this.GetAsync<TValue>(key, CancellationToken.None).Result;
    }

    public async Task<TValue> GetAsync<TValue>(string key, CancellationToken cancellationToken)
    {
        var item = await settings
            .Client
            .GetDatabase(settings.DatabaseId)
            .GetContainer(settings.ContainerId)
            .ReadItemAsync<CosmosDbItem<TValue>>(
                id: key,
                partitionKey: new PartitionKey(settings.PartitionKey), cancellationToken: cancellationToken);

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