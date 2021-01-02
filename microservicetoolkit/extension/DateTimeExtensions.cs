using System;

namespace mpstyle.microservice.toolkit
{
    public static class DateTimeExtensions
    {
        public static long ToEpoch(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUniversalTime().ToUnixTimeMilliseconds();
        }
    }
}
