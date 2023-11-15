using System;
using System.Net.Http;
using Fic.XTB.FlowExecutionHistory.Models.DTOs;
using Newtonsoft.Json;

namespace Fic.XTB.FlowExecutionHistory.Models
{
    public class FlowRun
    {
        public string Id { get; set; }
        public string CorrelationId { get; set; }
        public string FriendlyCorrelationId { get; set; }
        public string Status { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public int DurationInMilliseconds { get; set; }
        public string FormattedDuration { get; set; }
        public string Url { get; set; }
        public Flow Flow { get; set; }
        public FlowRunError Error { get; set; }
        public string TriggerOutputsUrl { get; set; }
        public TriggerOutputsResponseDto TriggerOutputs { get; set; }

        public TriggerOutputsResponseDto GetTriggerOutputs()
        {
            if (string.IsNullOrWhiteSpace(TriggerOutputsUrl)) { return null; }

            var client = new HttpClient();
            var response = client.GetAsync(TriggerOutputsUrl).Result;
            var responseJson = response.Content.ReadAsStringAsync().Result;
            var triggerOutputs = JsonConvert.DeserializeObject<TriggerOutputsResponseDto>(responseJson);

            TriggerOutputs = triggerOutputs;

            return triggerOutputs;
        }
    }
}
