using System;
using System.Runtime.Remoting.Messaging;

namespace Database
{
    public static class TimeService
    {
        public static Func<DateTime> GetNow = () => DateTime.UtcNow;

        public static void ResetToRealTime()
        {
            GetNow = () => DateTime.UtcNow;
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
