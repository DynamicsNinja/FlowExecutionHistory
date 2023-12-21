using System;
using System.Collections.Generic;

namespace Fic.XTB.FlowExecutionHistory.Models
{
    public class FlowRunsCache
    {
        public string FlowId { get; set; }
        public List<FlowRun> FlowRuns { get; set; }
        public DateTimeOffset QueryDateTIme { get; set; }
        public string StatusFilter { get; set; }
    }
}
