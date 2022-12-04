using microservice.toolkit.core;

using System.Text.Json;

namespace microservice.toolkit.cachemanager.serializer
{
    public class JsonCacheValueSerializer : ICacheValueSerializer
    {
        public TValue Deserialize<TValue>(string value)
        {
            return JsonSerializer.Deserialize<TValue>(value);
        }

        public string Serialize<TValue>(TValue value)
        {
            return JsonSerializer.Serialize(value);
        }
    }
}