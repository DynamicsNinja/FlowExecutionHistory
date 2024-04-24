using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Fic.XTB.FlowExecutionHistory.Models.AppInsights
{
    [DataContract]
    public class EventData
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public DateTime time { get; set; }
        [DataMember]
        public string iKey { get; set; }
        [DataMember]
        public Tags tags { get; set; }
        [DataMember]
        public Data data { get; set; }
    }

    [DataContract]
    public class Tags
    {
        [DataMember(Name = "ai.session.id")]
        public string SessionId { get; set; } = Guid.NewGuid().ToString();

        [DataMember(Name = "ai.operation.name")]
        public string OperationName { get; set; }

        [DataMember(Name = "ai.application.ver")]
        public string ApplicationVersion { get; set; }

        [DataMember(Name = "ai.device.osVersion")]
        public string OSVersion { get; set; }

        [DataMember(Name = "ai.device.type")]
        public string DeviceType { get; set; }
    }

    [DataContract]
    public class Data
    {
        [DataMember]
        public string baseType { get; set; }
        [DataMember]
        public Basedata baseData { get; set; }
    }

    [DataContract]
    public class Basedata
    {
        [DataMember]
        public int ver { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public Dictionary<string, string> properties { get; set; }
    } 
}
