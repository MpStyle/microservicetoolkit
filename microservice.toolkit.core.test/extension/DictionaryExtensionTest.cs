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
            Assert.IsTrue(DictionaryExtension.IsNullOrEmpty<object, object>(null));
            Assert.IsTrue(new Dictionary<object, object>().IsNullOrEmpty());
            Assert.IsFalse(new Dictionary<object, object>
            {
                {new object(), new object()}
            }.IsNullOrEmpty());
        }
    }
}