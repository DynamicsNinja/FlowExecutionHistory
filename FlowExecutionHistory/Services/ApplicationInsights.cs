using Fic.XTB.FlowExecutionHistory.Models.AppInsights;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

namespace Fic.XTB.FlowExecutionHistory.Services
{

    public class ApplicationInsights
    {
        private string _instrumentationKey = "60d281e8-bdca-4e33-aff9-e41225d53986";
        private string _url = "https://dc.services.visualstudio.com/v2/track";

        private string _sessionId;
        public ApplicationInsights()
        {
            _sessionId = Guid.NewGuid().ToString();
        }

        private string GetLastDotPart(string identifier)
        {
            return identifier == null ? null : !identifier.Contains(".") ? identifier : identifier.Substring(identifier.LastIndexOf('.') + 1);
        }

        public void LogEvent(string eventName)
        {
            var ed = new EventData
            {
                data = new Data
                {
                    baseData = new Basedata
                    {
                        name = eventName,
                        properties = new Dictionary<string, string>()

                    },
                    baseType = "EventData"
                },
                iKey = _instrumentationKey,
                name = "Microsoft.ApplicationInsights.Event",
                tags = new Tags
                {
                    DeviceType = GetLastDotPart(Assembly.GetExecutingAssembly().GetName().Name),
                    SessionId = _sessionId,
                    ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version.PaddedVersion(1, 4, 2, 2),
                    OSVersion = GetLastDotPart(Assembly.GetEntryAssembly().GetName().Name) + " " + Assembly.GetEntryAssembly().GetName().Version.PaddedVersion(1, 4, 2, 2)
                },
                time = DateTime.UtcNow,
            };

            var client = new HttpClient();
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(ed);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            client.PostAsync(_url, content);
        }
    }
}
