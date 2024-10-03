using microservice.toolkit.cachemanager.serializer;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.cachemanager.test.serializer
{
    [ExcludeFromCodeCoverage]
    public class XmlCacheValueSerializerTest
    {
        [Test]
        public void RoundTrip()
        {
            var serializer = new XmlCacheValueSerializer();
            var value = new MyGreetings { Greetings = "Hello World!" };
            var revalue = serializer.Deserialize<MyGreetings>(serializer.Serialize(value));
            Assert.That(value.Greetings, Is.EqualTo(revalue.Greetings));
        }

        public class MyGreetings
        {
            public string Greetings { get; init; }
        }
    }
}
