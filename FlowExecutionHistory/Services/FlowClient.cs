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

        private const string BaseUrl = "https://api.flow.microsoft.com";

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
            var urlBuilder = new UriBuilder(BaseUrl)
            {
                Path = $"/providers/Microsoft.ProcessSimple/environments/{_envId}/flows/{flowRun.Flow.Id}/runs/{flowRun.Id}/remediation",
                Query = "api-version=2016-11-01"
            };

            var responseJson = _client.GetAsync(urlBuilder.Uri).Result.Content.ReadAsStringAsync().Result;
            var flowRunRemediation = JsonConvert.DeserializeObject<FlowRunRemediationResponse>(responseJson);

            return flowRunRemediation;
        }

        public List<FlowRun> GetFlowRuns(Flow flow, string status, DateTimeOffset dateFrom, DateTimeOffset dateTo)
        {
            var flowRuns = new List<FlowRun>();
            var flowRunDtos = new List<FlowRunDto>();

            var dateFilter = $"StartTime gt {dateFrom.ToUniversalTime():s}Z";
            var statusFilter = status != "All" ? $"Status eq '{status}'" : string.Empty;

            var filterQuery = statusFilter == string.Empty
                ? $"$filter={dateFilter}"
                : $"$filter={statusFilter} and {dateFilter}";

            var urlBuilder = new UriBuilder(BaseUrl)
            {
                Path = $"/providers/Microsoft.ProcessSimple/environments/{_envId}/flows/{flow.Id}/runs/",
                Query = $"api-version=2016-11-01&$top=250&{filterQuery}"
            };

            var url = urlBuilder.Uri.ToString();

            while (!string.IsNullOrWhiteSpace(url))
            {
                var responseJson = _client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<FlowRunsResponseDto>(responseJson);
                flowRunDtos.AddRange(response?.value);

                url = response?.nextLink;
            }

            flowRunDtos = flowRunDtos
                .Where(fr => fr.properties.startTime <= dateTo)
                .ToList();

            foreach (var fr in flowRunDtos)
            {
                var duration = (fr.properties.endTime - fr.properties.startTime).TotalSeconds;
                var runUrl = $"https://make.powerautomate.com/environments/{_envId}/flows/{flow.Id}/runs/{fr.name}";

                flowRuns.Add(new FlowRun
                {
                    Id = fr.name,
                    Status = fr.properties.status,
                    StartDate = fr.properties.startTime.ToLocalTime(),
                    DurationInSeconds = (int)duration,
                    Url = runUrl,
                    Flow = flow,
                    CorrelationId = fr.properties.correlation.clientTrackingId,
                });
            }

            return flowRuns;
        }
    }
}
