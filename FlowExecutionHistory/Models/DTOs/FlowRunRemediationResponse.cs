namespace Fic.XTB.FlowExecutionHistory.Models.DTOs
{

    public class FlowRunRemediationResponse
    {
        //public string remediationType { get; set; }
        public string errorSubject { get; set; }
        public string errorDescription { get; set; }
        //public string operationOutputs { get; set; }
        //public string searchText { get; set; }
        //public Documentationlink documentationLink { get; set; }
    }

    public class Operationoutputs
    {
        public Error error { get; set; }
        public object body { get; set; }
    }

    public class Error
    {
        public string code { get; set; }
        public string message { get; set; }
        public string MicrosoftPowerAppsCDSErrorDetailsApiExceptionSourceKey { get; set; }
        public string MicrosoftPowerAppsCDSErrorDetailsApiStepKey { get; set; }
        public string MicrosoftPowerAppsCDSErrorDetailsApiDepthKey { get; set; }
        public string MicrosoftPowerAppsCDSErrorDetailsApiActivityIdKey { get; set; }
        public string MicrosoftPowerAppsCDSErrorDetailsApiPluginSolutionNameKey { get; set; }
        public string MicrosoftPowerAppsCDSErrorDetailsApiStepSolutionNameKey { get; set; }
        public string MicrosoftPowerAppsCDSErrorDetailsApiExceptionCategory { get; set; }
        public string MicrosoftPowerAppsCDSErrorDetailsApiExceptionMessageName { get; set; }
        public string MicrosoftPowerAppsCDSErrorDetailsApiExceptionHttpStatusCode { get; set; }
        public string MicrosoftPowerAppsCDSErrorDetailsDuplicateEntity { get; set; }
        public string MicrosoftPowerAppsCDSErrorDetailsDuplicateAttributes { get; set; }
        public string MicrosoftPowerAppsCDSHelpLink { get; set; }
        public string MicrosoftPowerAppsCDSInnerErrorMessage { get; set; }
    }

    public class Documentationlink
    {
        public string id { get; set; }
        public string name { get; set; }
        public Properties properties { get; set; }
    }

    public class Properties
    {
        public string description { get; set; }
        public string url { get; set; }
    }

}
