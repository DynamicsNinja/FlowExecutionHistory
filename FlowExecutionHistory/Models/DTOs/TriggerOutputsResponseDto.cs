using System.Collections.Generic;

namespace Fic.XTB.FlowExecutionHistory.Models.DTOs
{
    public class TriggerOutputsResponseDto
    {
        public Dictionary<string, object> Body { get; set; }
        public Dictionary<string, object> Headers { get; set; }
    }
}
