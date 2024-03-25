using microservice.toolkit.core;

using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace microservice.toolkit.cachemanager.serializer;

/// <summary>
/// Represents a cache value serializer that uses XML serialization.
/// </summary>
public class XmlCacheValueSerializer : ICacheValueSerializer
{
    private readonly XmlSerializerNamespaces emptyNamespaces
        = new XmlSerializerNamespaces([new XmlQualifiedName(string.Empty, string.Empty)]);

    /// <summary>
    /// Deserializes the specified XML string into an object of type TValue.
    /// </summary>
    /// <typeparam name="TValue">The type of the object to deserialize.</typeparam>
    /// <param name="value">The XML string to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public TValue Deserialize<TValue>(string value)
    {
        TValue instance;

        using (var reader = new StringReader(value))
        {
            var serializer = XmlSerializer.FromTypes([typeof(TValue)])[0];
            instance = (TValue)serializer.Deserialize(reader);
        }

        return instance;
    }

    /// <summary>
    /// Serializes the specified object into an XML string.
    /// </summary>
    /// <typeparam name="TValue">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized XML string.</returns>
    public string Serialize<TValue>(TValue value)
    {
        var content = string.Empty;

        using (var stream = new MemoryStream())
        {
            using (var writer = XmlWriter.Create(stream))
            {
                var serializer = XmlSerializer.FromTypes([typeof(TValue)])[0];

                serializer.Serialize(writer, value, emptyNamespaces);
            }

            stream.Flush();

            stream.Position = 0;

            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                content = reader.ReadToEnd();
            }
        }

        return content;
    }
}