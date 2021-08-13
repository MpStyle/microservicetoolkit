using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("mpstyle.microservice.toolkit.test")]
namespace mpstyle.microservice.toolkit
{
    internal static class DateTimeExtensions
    {
        internal static long ToEpoch(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUniversalTime().ToUnixTimeMilliseconds();
        }
    }
}
