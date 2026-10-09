using System;

namespace EvidenceApi.V1.Helpers
{
    public static class LondonDateTime
    {
        private static readonly TimeZoneInfo London =
            TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

        public static DateTime Now()
        {
            var londonTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, London);
            return DateTime.SpecifyKind(londonTime, DateTimeKind.Utc);
        }
    }
}
