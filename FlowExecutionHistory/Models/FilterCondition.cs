using Fic.XTB.FlowExecutionHistory.Enums;

namespace Fic.XTB.FlowExecutionHistory.Models
{
    public class FilterCondition
    {
        public string Attribute { get; set; }
        public OutputTriggerFilter Operator { get; set; }
        public string Value { get; set; }

        public FilterCondition(string attribute, OutputTriggerFilter op, string value)
        {
            Attribute = attribute;
            Operator = op;
            Value = value;
        }
    }
}
