using microservice.toolkit.core;

using System.Text.Json;

namespace microservice.toolkit.cachemanager.serializer;

/// <summary>
/// Represents a cache value serializer that uses JSON serialization.
/// </summary>
public class JsonCacheValueSerializer : ICacheValueSerializer
{
    private readonly JsonSerializerOptions jsonSerializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonCacheValueSerializer"/> class.
    /// </summary>
    /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
    public JsonCacheValueSerializer(JsonSerializerOptions jsonSerializerOptions = null)
    {
        this.jsonSerializerOptions = jsonSerializerOptions;
    }

    /// <summary>
    /// Deserializes the specified value into an object of type <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the object to deserialize.</typeparam>
    /// <param name="value">The serialized value to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public TValue Deserialize<TValue>(string value)
    {
        return JsonSerializer.Deserialize<TValue>(value, this.jsonSerializerOptions);
    }

    /// <summary>
    /// Serializes the specified value into a JSON string.
    /// </summary>
    /// <typeparam name="TValue">The type of the object to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The serialized JSON string.</returns>
    public string Serialize<TValue>(TValue value)
    {
        return JsonSerializer.Serialize(value, this.jsonSerializerOptions);
    }
}