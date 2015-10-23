using System;
using System.Runtime.Remoting.Messaging;

namespace Database
{
    public static class TimeService
    {
        public static Func<DateTime> GetUtcNow = () => DateTime.UtcNow;

        public static void ResetToRealTime()
        {
            GetUtcNow = () => DateTime.UtcNow;
        }

        public static DateTime UtcNow
        {
            get
            {
                return GetUtcNow();
            }
            set
            {
                GetUtcNow = () => value;
            }
        }

        public static void AddSeconds(int seconds)
        {
            UtcNow = UtcNow.AddSeconds(seconds);
        }
    }
}
