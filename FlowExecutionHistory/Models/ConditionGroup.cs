using Fic.XTB.FlowExecutionHistory.Enums;
using System.Collections.Generic;

namespace Fic.XTB.FlowExecutionHistory.Models
{
    public class ConditionGroup
    {
        public List<FilterCondition> FilterConditions { get; set; }
        public GroupOperator GroupOperator { get; set; }
    }
}
