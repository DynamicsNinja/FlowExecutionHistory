using Fic.XTB.FlowExecutionHistory.Enums;

namespace Fic.XTB.FlowExecutionHistory.Helpers
{
    public static class FlowEndpointHelper
    {
        public static string GetAuthorityUrl(OrganizationGeo geo)
        {
            switch (geo)
            {
                case OrganizationGeo.GCC:
                case OrganizationGeo.GCCH:
                case OrganizationGeo.DOD:
                    return "https://login.microsoftonline.us";
                default:
                    return "https://login.microsoftonline.com";
            }
        }


        public static string GetAudienceUrl(OrganizationGeo geo)
        {
            switch (geo)
            {
                case OrganizationGeo.GCC:
                    return "https://gov.service.flow.microsoft.us";
                case OrganizationGeo.GCCH:
                    return "https://high.service.flow.microsoft.us";
                case OrganizationGeo.DOD:
                    return "https://service.flow.appsplatform.us";
                default:
                    return "https://service.flow.microsoft.com";
            }
        }

        public static string GetFlowApiBaseUrl(OrganizationGeo geo)
        {
            switch (geo)
            {
                case OrganizationGeo.GCC:
                    return "https://gov.api.flow.microsoft.us";
                case OrganizationGeo.GCCH:
                    return "https://high.api.flow.microsoft.us";
                case OrganizationGeo.DOD:
                    return "https://api.flow.appsplatform.us";
                default:
                    return "https://api.flow.microsoft.com";
            }
        }

        public static string GetMakerUrl(OrganizationGeo geo)
        {
            switch (geo)
            {
                case OrganizationGeo.GCC:
                    return "https://make.gov.powerautomate.us";
                case OrganizationGeo.GCCH:
                    return "https://make.high.powerautomate.us";
                case OrganizationGeo.DOD:
                    return "https://make.powerautomate.appsplatform.us";
                default:
                    return "https://make.powerautomate.com";
            }
        }
    }
}
