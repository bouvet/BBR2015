using System;
using System.Runtime.Remoting.Messaging;

namespace Database
{
    public static class TimeService
    {
        private static TimeZoneInfo cetZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");

        public static Func<DateTime> GetNow = () => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cetZone);

        public static void ResetToRealTime()
        {
            GetNow = () => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cetZone);
        }

        public static DateTime Now
        {
            get
            {
                return GetNow();
            }
            set
            {
                GetNow = () => value;
            }
        }

        public static void AddSeconds(int seconds)
        {
            Now = Now.AddSeconds(seconds);
        }
    }
}
