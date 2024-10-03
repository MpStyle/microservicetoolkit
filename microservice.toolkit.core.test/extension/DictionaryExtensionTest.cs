using microservice.toolkit.core.extension;

using NUnit.Framework;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.core.test.extension
{
    [ExcludeFromCodeCoverage]
    public class DictionaryExtensionTest
    {
        [Test]
        public void IsNullOrEmpty()
        {
            Assert.That(DictionaryExtension.IsNullOrEmpty<object, object>(null), Is.True);
            Assert.That(new Dictionary<object, object>().IsNullOrEmpty(), Is.True);
            Assert.That(new Dictionary<object, object>
            {
                {new object(), new object()}
            }.IsNullOrEmpty(), Is.False);
        }
    }
}