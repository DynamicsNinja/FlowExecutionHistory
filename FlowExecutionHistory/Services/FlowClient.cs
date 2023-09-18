using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Fic.XTB.FlowExecutionHistory.Models;
using Fic.XTB.FlowExecutionHistory.Models.DTOs;
using Newtonsoft.Json;

namespace Fic.XTB.FlowExecutionHistory.Services
{
    public class FlowClient
    {
        private readonly HttpClient _client;
        private readonly string _envId;

        private string baseUrl = "https://api.flow.microsoft.com";

        public FlowClient(string envId, string accessToken)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            _client = client;
            _envId = envId;
        }

        public FlowRunRemediationResponse GetFlowRunErrorDetails(FlowRun flowRun)
        {
            var url =
                $"{baseUrl}/providers/Microsoft.ProcessSimple/environments/{_envId}/flows/{flowRun.Flow.Id}/runs/{flowRun.Id}/remediation?api-version=2016-11-01";

            var responseJson = _client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
            var flowRunRemediation = JsonConvert.DeserializeObject<FlowRunRemediationResponse>(responseJson);

            return flowRunRemediation;
        }

        public List<FlowRun> GetFlowRuns(Flow flow, string status, DateTime dateFrom, DateTime dateTo)
        {
            var flowRuns = new List<FlowRun>();
            var flowRunDtos = new List<FlowRunDto>();

            var dateFilter = $"StartTime gt {dateFrom:s}Z";
            var statusFilter = status != "All" ? $"Status eq '{status}'" : string.Empty;

            var filterQuery = statusFilter == string.Empty
                ? $"$filter={dateFilter}"
                : $"$filter={statusFilter} and {dateFilter}";
            var url =
                $"{baseUrl}/providers/Microsoft.ProcessSimple/environments/{_envId}/flows/{flow.Id}/runs/?api-version=2016-11-01&$top=250&{filterQuery}";


            while (!string.IsNullOrWhiteSpace(url))
            {
                var responseJson = _client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<FlowRunsResponseDto>(responseJson);
                flowRunDtos.AddRange(response?.value);

                url = response?.nextLink;
            }

            flowRunDtos = flowRunDtos.Where(fr => fr.properties.startTime <= dateTo).ToList();

            foreach (var fr in flowRunDtos)
            {
                var duration = (fr.properties.endTime - fr.properties.startTime).TotalSeconds;
                var runUrl = $"https://make.powerautomate.com/environments/{_envId}/flows/{flow.Id}/runs/{fr.name}";

                flowRuns.Add(new FlowRun
                {
                    Id = fr.name,
                    Status = fr.properties.status,
                    StartDate = fr.properties.startTime,
                    DurationInSeconds = (int)duration,
                    Url = runUrl,
                    Flow = flow
                });
            }

            return flowRuns;
        }

    }
}
