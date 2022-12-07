using microservice.toolkit.cachemanager.serializer;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.cachemanager.test.serializer
{
    [ExcludeFromCodeCoverage]
    public class JsonCacheValueSerializerTest
    {
        [Test]
        public void RoundTrip()
        {
            var serializer = new JsonCacheValueSerializer();
            var value = new MyGreetings { Greetings = "Hello World!" };
            var revalue = serializer.Deserialize<MyGreetings>(serializer.Serialize(value));
            Assert.AreEqual(value.Greetings, revalue.Greetings);
        }

        class MyGreetings
        {
            public string Greetings { get; set; }
        }
    }
}
