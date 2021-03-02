using System;

namespace mpstyle.microservice.toolkit.book
{
    public class DateTimeUtils
    {
        /// <summary>
        /// Uses and requires UTC timezone.
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var toReturn = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(unixTimeStamp);
            return toReturn;
        }
    }
}
