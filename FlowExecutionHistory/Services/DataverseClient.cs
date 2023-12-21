using Fic.XTB.FlowExecutionHistory.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
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

        public List<Guid> GetSolutionComponents(Guid solutionId)
        {
            var fetch = $@"
            <fetch>
              <entity name='solutioncomponent'>
                <attribute name='objectid' />
                <filter>
                  <condition attribute='componenttype' operator='eq' value='29' />
                  <condition attribute='solutionid' operator='eq' value='{solutionId:D}' />
                </filter>
              </entity>
            </fetch>";

            var entities = _service.RetrieveMultiple(new FetchExpression(fetch)).Entities.ToList();

            var flowIds = new List<Guid>();
            foreach (var entity in entities)
            {
                flowIds.Add(entity.GetAttributeValue<Guid>("objectid"));
            }

            return flowIds;
        }

        public List<Solution> GetSolutions()
        {
            var fetch = $@"
            <fetch version='1.0' mapping='logical' savedqueryid='5bec9251-8956-4db6-beb8-f9c048c667e6'>
              <entity name='solution'>
                <attribute name='friendlyname' />
                <attribute name='uniquename' />
                <order attribute='friendlyname' />
              </entity>
            </fetch>";

            var entities = _service.RetrieveMultiple(new FetchExpression(fetch)).Entities.ToList();

            var solutions = new List<Solution>();

            foreach (var entity in entities)
            {
                var solution = new Solution
                {
                    Id = entity.Id,
                    Name = entity.GetAttributeValue<string>("friendlyname")
                };

                solutions.Add(solution);
            }

            return solutions;
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
