using Fic.XTB.FlowExecutionHistory.Enums;
using System;
using System.Collections.Generic;

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

        public bool Evaluate(Dictionary<string, object> data)
        {
            var comparisonOperators = new Dictionary<OutputTriggerFilter, Func<string, string, bool>>
            {
                { OutputTriggerFilter.Equals, (a, b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase) },
                { OutputTriggerFilter.NotEquals, (a, b) => !string.Equals(a, b, StringComparison.OrdinalIgnoreCase) },
                { OutputTriggerFilter.Contains, (a, b) => a.Contains(b) },
                { OutputTriggerFilter.NotContains, (a, b) => !a.Contains(b) },
                { OutputTriggerFilter.StartsWith, (a, b) => a.StartsWith(b) },
                { OutputTriggerFilter.EndsWith, (a, b) => a.EndsWith(b) }
            };

            if (!data.ContainsKey(Attribute)) { return false; }

            var value = data[Attribute]?.ToString()?.ToLowerInvariant();

            if (value == null) { return false; }

            var comparisonFound = comparisonOperators.TryGetValue(Operator, out var comparisonFunction);

            if (!comparisonFound) { return false; }

            var filterValue = Value?.ToLowerInvariant();

            var result = comparisonFunction(value, filterValue);

            return result;
        }
    }
}
