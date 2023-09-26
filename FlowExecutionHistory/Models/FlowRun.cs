using System;
using System.Collections.Generic;
using System.Net.Http;
using Fic.XTB.FlowExecutionHistory.Models.DTOs;
using Newtonsoft.Json;

namespace Fic.XTB.FlowExecutionHistory.Models
{
    public class FlowRun
    {
        public string Id { get; set; }
        public string CorrelationId { get; set; }
        public string Status { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public double DurationInSeconds { get; set; }
        public string Url { get; set; }
        public Flow Flow { get; set; }
        public FlowRunError Error { get; set; }
        public string TriggerOutputsUrl { get; set; }

        public TriggerOutputsResponseDto GetTriggerOutputs()
        {
            var client = new HttpClient();
            var response = client.GetAsync(TriggerOutputsUrl).Result;
            var responseJson = response.Content.ReadAsStringAsync().Result;
            var triggerOutputs = JsonConvert.DeserializeObject<TriggerOutputsResponseDto>(responseJson);

            return triggerOutputs;
        }
    }
}
