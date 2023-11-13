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

            var hours = timeSpan.Hours;
            var minutes = timeSpan.Minutes;
            var seconds = timeSpan.Seconds;

            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
    }
}
