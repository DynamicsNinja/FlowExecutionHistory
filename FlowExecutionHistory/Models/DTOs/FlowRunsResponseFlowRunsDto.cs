using System;
using System.Collections.Generic;

namespace Fic.XTB.FlowExecutionHistory.Models.DTOs
{
    public class FlowRunsResponseDto
    {
        public List<FlowRunDto> value { get; set; }
        public string nextLink { get; set; }

    }

    public class FlowRunDto
    {
        public string name { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public FlowRunPropertiesDto properties { get; set; }
    }

    public class FlowRunPropertiesDto
    {
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string status { get; set; }
        public string code { get; set; }
        public FlowRunErrorDto error { get; set; }
        public FlowRunCorrelationDto correlation { get; set; }
        public FlowRunTriggerDto trigger { get; set; }
    }

    public class FlowRunErrorDto
    {
        public string code { get; set; }
        public string message { get; set; }
    }

    public class FlowRunCorrelationDto
    {
        public string clientTrackingId { get; set; }
    }

    public class FlowRunTriggerDto
    {
        public string name { get; set; }
        public InputslinkDto inputsLink { get; set; }
        public OutputslinkDto outputsLink { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string originHistoryName { get; set; }
        public Correlation1Dto correlation { get; set; }
        public string status { get; set; }
    }

    public class InputslinkDto
    {
        public string uri { get; set; }
        public string contentVersion { get; set; }
        public int contentSize { get; set; }
        public ContenthashDto contentHash { get; set; }
    }

    public class ContenthashDto
    {
        public string algorithm { get; set; }
        public string value { get; set; }
    }

    public class OutputslinkDto
    {
        public string uri { get; set; }
        public string contentVersion { get; set; }
        public int contentSize { get; set; }
        public Contenthash1Dto contentHash { get; set; }
    }

    public class Contenthash1Dto
    {
        public string algorithm { get; set; }
        public string value { get; set; }
    }

    public class Correlation1Dto
    {
        public string clientTrackingId { get; set; }
    }

}
