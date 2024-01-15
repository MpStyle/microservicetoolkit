using microservice.toolkit.core;

using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace microservice.toolkit.cachemanager.serializer;

public class XmlCacheValueSerializer : ICacheValueSerializer
{
    private readonly XmlSerializerNamespaces emptyNamespaces
        = new XmlSerializerNamespaces(new[] { new XmlQualifiedName(string.Empty, string.Empty) });

    public TValue Deserialize<TValue>(string value)
    {
        TValue instance;

        using (var reader = new StringReader(value))
        {
            var serializer = XmlSerializer.FromTypes(new[] { typeof(TValue) })[0];
            instance = (TValue)serializer.Deserialize(reader);
        }

        return instance;
    }

    public string Serialize<TValue>(TValue value)
    {
        var content = string.Empty;

        using (var stream = new MemoryStream())
        {
            using (var writer = XmlWriter.Create(stream))
            {
                var serializer = XmlSerializer.FromTypes(new[] { typeof(TValue) })[0];

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