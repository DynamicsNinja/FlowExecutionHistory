using System;

namespace Fic.XTB.FlowExecutionHistory.Models
{
    public class FlowRun
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public double DurationInSeconds { get; set; }
        public string Url { get; set; }
        public Flow Flow { get; set; }
    }
}
