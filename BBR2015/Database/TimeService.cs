using System;
using System.Runtime.Remoting.Messaging;

namespace Database
{
    public static class TimeService
    {
        public static Func<DateTime> GetNow = () => DateTime.Now;

        public static void ResetToRealTime()
        {
            GetNow = () => DateTime.Now;
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
