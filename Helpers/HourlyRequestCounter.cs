using System;

namespace MZDNETWORK.Helpers
{
    public static class HourlyRequestCounter
    {
        private static readonly int[] _requests = new int[24];
        private static int _currentHour = DateTime.UtcNow.Hour;
        private static readonly object _lock = new object();

        public static void Increment()
        {
            lock (_lock)
            {
                int hour = DateTime.UtcNow.Hour;
                if (hour != _currentHour)
                {
                    // reset count for new hour
                    _requests[hour] = 0;
                    _currentHour = hour;
                }
                _requests[hour]++;
            }
        }

        public static int[] GetSnapshot()
        {
            lock (_lock)
            {
                return (int[])_requests.Clone();
            }
        }
    }
} 