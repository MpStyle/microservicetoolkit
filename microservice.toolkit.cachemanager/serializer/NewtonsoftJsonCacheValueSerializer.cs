using microservice.toolkit.core;

using Newtonsoft.Json;

namespace microservice.toolkit.cachemanager.serializer
{
    public class NewtonsoftJsonCacheValueSerializer : ICacheValueSerializer
    {
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public NewtonsoftJsonCacheValueSerializer(JsonSerializerSettings jsonSerializerSettings = null)
        {
            this.jsonSerializerSettings = jsonSerializerSettings;
        }

        public TValue Deserialize<TValue>(string value)
        {
            return JsonConvert.DeserializeObject<TValue>(value, this.jsonSerializerSettings);
        }

        public string Serialize<TValue>(TValue value)
        {
            return JsonConvert.SerializeObject(value, this.jsonSerializerSettings);
        }
    }
}