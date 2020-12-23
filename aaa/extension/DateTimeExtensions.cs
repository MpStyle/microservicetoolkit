using System;

namespace mpstyle.microservice.toolkit
{
    public static class DateTimeExtensions
    {
        public static long ToEpoch(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUniversalTime().ToUnixTimeMilliseconds();
        }

        public static string ToFormatedDate(this long timestamp, string format)
        {
            return (new DateTime(1970, 1, 1)).AddMilliseconds(timestamp).ToString(format);
        }
    }
}
