using System;

namespace eNetwork.Framework.Extensions
{
    public static class TimeExtensions
    {
        public static long ToUnix(this DateTime time)
        {
            DateTimeOffset dto = new DateTimeOffset(time.ToUniversalTime());
            return dto.ToUnixTimeSeconds();
        }
    }
}
