using System.Collections.Generic;

namespace Fic.XTB.FlowExecutionHistory.Models
{

    public class FlowClientData
    {
        public Properties properties { get; set; }
    }

    public class Properties
    {
        public Definition definition { get; set; }
    }


    public class Definition
    {
        public Dictionary<string, TriggerInfo> triggers { get; set; }
    }

    public class TriggerInfo
    {
        public string type { get; set; }
    }
}
