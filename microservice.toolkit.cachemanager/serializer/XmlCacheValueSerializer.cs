using microservice.toolkit.core;

using System.Text.Json;

namespace microservice.toolkit.cachemanager.serializer
{
    public class JsonCacheValueSerializer : ICacheValueSerializer
    {
        private readonly JsonSerializerOptions jsonSerializerOptions = null;

        public JsonCacheValueSerializer(JsonSerializerOptions jsonSerializerOptions = null)
        {
            this.jsonSerializerOptions = jsonSerializerOptions;
        }

        public TValue Deserialize<TValue>(string value)
        {
            return JsonSerializer.Deserialize<TValue>(value, this.jsonSerializerOptions);
        }

        public string Serialize<TValue>(TValue value)
        {
            return JsonSerializer.Serialize(value, this.jsonSerializerOptions);
        }
    }
}