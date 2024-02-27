using System;

namespace Fic.XTB.FlowExecutionHistory.Helpers
{
    public static class TimeFormatter
    {
        public static string MillisecondsTimeString(int totalMilliseconds)
        {
            if (totalMilliseconds < 1000)
            {
                return $"{totalMilliseconds} ms";
            }

            var totalSeconds = totalMilliseconds / 1000;

            var timeSpan = TimeSpan.FromSeconds(totalSeconds);

            var days = timeSpan.Days;
            var hours = timeSpan.Hours;
            var minutes = timeSpan.Minutes;
            var seconds = timeSpan.Seconds;

            if (days > 0)
            {
                return $"{days}d {hours:D2}:{minutes:D2}:{seconds:D2}";
            }

            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
    }
}
