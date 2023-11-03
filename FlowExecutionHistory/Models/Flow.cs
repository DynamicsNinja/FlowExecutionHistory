using Fic.XTB.FlowExecutionHistory.Enums;
using System.Collections.Generic;
using System.Drawing;

namespace Fic.XTB.FlowExecutionHistory.Models
{
    public class Flow
    {
        public FlowStatus Status { get; set; }
        public bool IsSelected { get; set; }
        public Color? Color { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public FlowClientData ClientData { get; set; }
        public List<FlowRun> FlowRuns { get; set; }
        public string TriggerType { get; set; }
        public bool IsManaged { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
