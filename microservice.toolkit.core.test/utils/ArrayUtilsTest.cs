using microservice.toolkit.core.extension;

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
            var array = new int[] { 0, 1 }.ConcatArrays(
                [2, 3],
                [4, 5],
                [6, 7],
                [8, 9]
            );

            Assert.That(10, Is.EqualTo(array.Length));
            for (var i = 0; i < array.Length; i++)
            {
                Assert.That(i, Is.EqualTo(array[i]));
            }
        }
    }
}