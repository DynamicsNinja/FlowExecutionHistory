using Fic.XTB.FlowExecutionHistory.Enums;
using System.Collections.Generic;

namespace Fic.XTB.FlowExecutionHistory.Models
{
    public class ConditionGroup
    {
        public List<FilterCondition> FilterConditions { get; set; }
        public GroupOperator GroupOperator { get; set; }

        public bool Evaluate(Dictionary<string, object> dict)
        {
            var isMatch = false;

            foreach (var filterCondition in FilterConditions)
            {
                var conditionMatch = filterCondition.Evaluate(dict);

                if (GroupOperator == GroupOperator.And && !conditionMatch)
                {
                    isMatch = false;
                    break;
                }

                if (GroupOperator == GroupOperator.Or && conditionMatch)
                {
                    isMatch = true;
                    break;
                }

                isMatch = conditionMatch;
            }

            return isMatch;
        }

    }
}
