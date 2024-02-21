using Fic.XTB.FlowExecutionHistory.Models;

namespace Fic.XTB.FlowExecutionHistory
{
    public class Settings
    {
        public Browser Browser { get; set; }
        public BrowserProfile BrowserProfile { get; set; }
        public bool UseFlowColors { get; set; } = true;
        public bool ShowFriendlyCorrelationIds { get; set; } = true;
        public bool ShowErrorColumn { get; set; } = false;
    }
}