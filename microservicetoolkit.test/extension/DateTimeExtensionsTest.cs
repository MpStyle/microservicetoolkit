using System;

using Xunit;

namespace mpstyle.microservice.toolkit.test.extension
{
    public class DateTimeExtensionsTest
    {
        [Fact]
        public void ToEpoch()
        {
            Assert.Equal(0, new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToEpoch());
        }
    }
}