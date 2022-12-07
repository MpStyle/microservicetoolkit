using microservice.toolkit.core.utils;

using NUnit.Framework;

using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.core.test.utils
{
    [ExcludeFromCodeCoverage]
    public class ArrayUtilsTest
    {
        [Test]
        public void ConcatArrays()
        {
            var array = ArrayUtils.ConcatArrays(
                new[] { 0, 1 },
                new[] { 2, 3 },
                new[] { 4, 5 },
                new[] { 6, 7 },
                new[] { 8, 9 }
            );

            Assert.AreEqual(10, array.Length);
            for (var i = 0; i < array.Length; i++)
            {
                Assert.AreEqual(i, array[i]);
            }
        }
    }
}