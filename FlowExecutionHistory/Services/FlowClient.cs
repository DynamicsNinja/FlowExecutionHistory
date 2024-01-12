using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Fic.XTB.FlowExecutionHistory.Enums;
using Fic.XTB.FlowExecutionHistory.Helpers;
using Fic.XTB.FlowExecutionHistory.Models;
using Fic.XTB.FlowExecutionHistory.Models.DTOs;
using Newtonsoft.Json;

namespace Fic.XTB.FlowExecutionHistory.Services
{
    public class FlowClient
    {
        private readonly HttpClient _client;
        private readonly string _envId;

        public Dictionary<string, TriggerOutputsResponseDto> CachedTriggerOutputs = new Dictionary<string, TriggerOutputsResponseDto>();

        public List<FlowRunsCache> FlowRunsCache { get; set; }

        private string BaseUrl { get; set; }
        private string MakePowerAutomateUrl { get; set; }

        public FlowClient(string envId, string accessToken, OrganizationGeo geo)
        {
            BaseUrl = FlowEndpointHelper.GetFlowApiBaseUrl(geo);
            MakePowerAutomateUrl = FlowEndpointHelper.GetMakerUrl(geo);
            FlowRunsCache = new List<FlowRunsCache>();
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
            var flowRunRemediationResponse = JsonConvert.DeserializeObject<FlowRunRemediationResponse>(responseJson);

            return flowRunRemediationResponse;
        }

        public List<FlowRun> GetFlowRunsFromApi(Flow flow, string status, DateTimeOffset dateFrom, bool includeTriggerOutputs)
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
                var response = _client.GetAsync(url).Result;
                var responseJson = response.Content.ReadAsStringAsync().Result;
                var flowRunsResponse = JsonConvert.DeserializeObject<FlowRunsResponseDto>(responseJson);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"{flowRunsResponse?.error.code}: {flowRunsResponse?.error.message}");
                }

                if (flowRunsResponse?.value != null)
                {
                    flowRunDtos.AddRange(flowRunsResponse.value);
                }

                url = flowRunsResponse?.nextLink;
            }

            foreach (var fr in flowRunDtos)
            {
                var duration = (fr.properties.endTime - fr.properties.startTime).TotalMilliseconds;
                var runUrl = $"{MakePowerAutomateUrl}/environments/{_envId}/flows/{flow.Id}/runs/{fr.name}";

                var corrId = fr.properties.correlation.clientTrackingId;

                var flowRun = new FlowRun
                {
                    Id = fr.name,
                    Status = fr.properties.status,
                    StartDate = fr.properties.startTime.ToLocalTime(),
                    EndDate = fr.properties.endTime.ToLocalTime(),
                    DurationInMilliseconds = (int)duration,
                    Url = runUrl,
                    Flow = flow,
                    CorrelationId = corrId,
                    TriggerOutputsUrl = fr.properties.trigger.outputsLink?.uri
                };

                flowRuns.Add(flowRun);
            }

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 4
            };


            if (includeTriggerOutputs)
            {
                Parallel.ForEach(flowRuns, options,
                    fr =>
                    {
                        fr.TriggerOutputs = fr.TriggerOutputs ?? GetTriggerOutputsForFlowRun(fr);

                    });

                foreach (var fr in flowRuns)
                {
                    CachedTriggerOutputs[fr.Flow.Id] = fr.TriggerOutputs;
                }
            }

            flowRuns = FriendlyfyCorrelationIds(flowRuns);

            AddFlowRunsToCache(flow.Id, status, dateFrom, flowRuns);

            return flowRuns;
        }

        public void AddFlowRunsToCache(string flowId, string status, DateTimeOffset dateFrom, List<FlowRun> flowRuns)
        {
            var flowRunsCache = new FlowRunsCache
            {
                FlowId = flowId,
                FlowRuns = flowRuns,
                QueryDateTIme = dateFrom,
                StatusFilter = status
            };

            FlowRunsCache.Add(flowRunsCache);
        }

        public List<FlowRun> GetFlowRunsFromCache(string flowId, string status, DateTimeOffset dateFrom)
        {
            var flowRunsCache = FlowRunsCache.FirstOrDefault(x => x.FlowId == flowId && x.StatusFilter == "All" && x.QueryDateTIme <= dateFrom)
                ?? FlowRunsCache.FirstOrDefault(x => x.FlowId == flowId && x.StatusFilter == status && x.QueryDateTIme <= dateFrom);

            return flowRunsCache?.FlowRuns;
        }

        public List<FlowRun> GetFlowRuns(Flow flow, string status, DateTimeOffset dateFrom, DateTimeOffset dateTo, int durationThreshold, bool includeTriggerOutputs)
        {
            var flowRuns = GetFlowRunsFromCache(flow.Id, status, dateFrom) ?? GetFlowRunsFromApi(flow, status, dateFrom, includeTriggerOutputs);

            flowRuns = flowRuns
                .Where(x => x.StartDate >= dateFrom && x.StartDate <= dateTo)
                .Where(r => r.DurationInMilliseconds / 1000 >= durationThreshold)
                .ToList();

            if (status != "All")
            {
                flowRuns = flowRuns.Where(x => x.Status == status).ToList();
            }

            return flowRuns;
        }

        public TriggerOutputsResponseDto GetTriggerOutputsForFlowRun(FlowRun flowRun)
        {
            var triggerOutputs = CachedTriggerOutputs.TryGetValue(flowRun.Id, out var to) ? to : flowRun.GetTriggerOutputs();

            return triggerOutputs;
        }

        private List<FlowRun> FriendlyfyCorrelationIds(List<FlowRun> flowRuns)
        {
            var friendlyIds = new Dictionary<string, string>();

            foreach (var flowRun in flowRuns.OrderByDescending(fr => fr.StartDate))
            {
                var corrId = flowRun.CorrelationId;

                string friendlyId;

                if (friendlyIds.TryGetValue(corrId, out var id))
                {
                    friendlyId = id;
                }
                else
                {
                    var newFriendlyId = GenerateFriendlyId(friendlyIds.Keys.Count + 1);
                    friendlyId = newFriendlyId;
                    friendlyIds.Add(corrId, newFriendlyId);
                }

                flowRun.FriendlyCorrelationId = friendlyId;
            }
            return flowRuns;
        }

        public static string GenerateFriendlyId(int n)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string id = string.Empty;

            while (n > 0)
            {
                n--; // adjust zero index based
                int remainder = n % 26;
                id = alphabet[remainder] + id;
                n /= 26;
            }

            return id;
        }
    }
}
