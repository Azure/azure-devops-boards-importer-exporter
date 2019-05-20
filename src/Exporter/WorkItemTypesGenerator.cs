using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using System;

namespace AzdoBoardsManager.Exporter
{
    public class WorkItemTypesGenerator : Base
    {
        private ProcessInfo _process;
       
        public WorkItemTypesGenerator(ProcessInfo process)
        {
            _process = process;
        }

        public override void Generate(VssConnection connection)
        {
           var witClient = connection.GetClient<WorkItemTrackingProcessHttpClient>();
           var workItemTypes = witClient.GetWorkItemTypesAsync(_process.TypeId).SyncResult(); //workItemType expand states
                

            foreach (var workItemType in workItemTypes)
            {
                if (workItemType.Id.StartsWith("Microsoft"))
                {
                    continue;
                }

                if (workItemType.IsDisabled == true)
                {
                    continue;
                }

                SaveObject<Models.WorkItemType>(workItemType);

                var statesGenerator = new WorkItemTypeStatesGenerator(_process, workItemType.Id)
                {
                    BaseFolder = BaseFolder
                };
                statesGenerator.Generate(connection);
            }
        }

        public override void ProcessDefinition<T>(T definition)
        {
            var workItemType = definition as Models.WorkItemType;
            workItemType.ProcessId = _process.ReferenceName ?? _process.Name;
        }
    }
}
