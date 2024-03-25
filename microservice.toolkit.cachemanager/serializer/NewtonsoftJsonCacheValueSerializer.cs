using microservice.toolkit.core;

using Newtonsoft.Json;

namespace microservice.toolkit.cachemanager.serializer;

/// <summary>
/// Provides a cache value serializer using Newtonsoft.Json library.
/// </summary>
public class NewtonsoftJsonCacheValueSerializer : ICacheValueSerializer
{
    private readonly JsonSerializerSettings jsonSerializerSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="NewtonsoftJsonCacheValueSerializer"/> class.
    /// </summary>
    /// <param name="jsonSerializerSettings">The optional JsonSerializerSettings to be used for serialization and deserialization.</param>
    public NewtonsoftJsonCacheValueSerializer(JsonSerializerSettings jsonSerializerSettings = null)
    {
        this.jsonSerializerSettings = jsonSerializerSettings;
    }

    /// <summary>
    /// Deserializes the specified value into an object of type <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the object to deserialize.</typeparam>
    /// <param name="value">The serialized value to be deserialized.</param>
    /// <returns>The deserialized object.</returns>
    public TValue Deserialize<TValue>(string value)
    {
        return JsonConvert.DeserializeObject<TValue>(value, this.jsonSerializerSettings);
    }

    /// <summary>
    /// Serializes the specified value into a string representation.
    /// </summary>
    /// <typeparam name="TValue">The type of the object to serialize.</typeparam>
    /// <param name="value">The value to be serialized.</param>
    /// <returns>The serialized string representation of the value.</returns>
    public string Serialize<TValue>(TValue value)
    {
        return JsonConvert.SerializeObject(value, this.jsonSerializerSettings);
    }
}