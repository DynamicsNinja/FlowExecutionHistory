using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.Linq;

namespace Fic.XTB.FlowExecutionHistory.Services
{
    public class DataverseClient
    {
        private readonly IOrganizationService _service;
        public DataverseClient(IOrganizationService service)
        {
            _service = service;
        }

        public List<Entity> GetFlows()
        {
            var fetch = $@"
                    <fetch>
                      <entity name='workflow'>
                        <attribute name='workflowid' />
                        <attribute name='workflowidunique' />
                        <attribute name='clientdata' />
                        <attribute name='name' />
                        <attribute name='statecode' />
                        <attribute name='statuscode' />
                        <attribute name='ismanaged' />
                        <filter type='and'>
                          <condition attribute='category' operator='eq' value='5' />
                        </filter>
                      </entity>
                    </fetch>";

            var flows = _service.RetrieveMultiple(new FetchExpression(fetch)).Entities.ToList();

            return flows;
        }
    }
}
