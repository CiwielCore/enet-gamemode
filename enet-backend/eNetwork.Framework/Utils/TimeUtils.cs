using System;

namespace eNetwork.Framework.Utils
{
    public static class TimeUtils
    {
        public static DateTime UnixTimeToDateTime(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;
        }
    }
}
