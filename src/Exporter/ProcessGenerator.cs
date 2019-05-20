using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi.Models;
using System;
using System.Collections.Generic;

namespace AzdoBoardsManager.Exporter
{
    public class ProcessGenerator : Base
    {
        private Dictionary<Guid, string> ProcessGuidMap;
        public ProcessGenerator()
        {

        }

        public override void Generate(VssConnection connection)
        {
            var processClient = connection.GetClient<WorkItemTrackingProcessHttpClient>();
            var processes = processClient.GetListOfProcessesAsync().SyncResult();

            ProcessGuidMap = new Dictionary<Guid, string>();

            // Add the set to the full list
            // Iterate and show the name of each project
            foreach (var process in processes)
            {
                var processId = GetProcessId(process);
                ProcessGuidMap.Add(process.TypeId, processId);
            }

            foreach (var process in processes)
            {
                if (string.IsNullOrWhiteSpace(process.ReferenceName))
                {
                    continue;
                }

                var processId = GetProcessId(process);

                SaveObject<Models.Process>(process);

                var workItemGenerator = new WorkItemTypesGenerator(process)
                {
                    BaseFolder = BaseFolder
                };

                workItemGenerator.Generate(connection);
            }
        }

        public override void ProcessDefinition<T>(T definition)
        {
            var process = definition as Models.Process;
            if (process.ParentProcessTypeId != null && ProcessGuidMap.ContainsKey(process.ParentProcessTypeId))
            {
                process.ParentProcessId = ProcessGuidMap[process.ParentProcessTypeId];
            }
        }

        private string GetProcessId(ProcessInfo process)
        {
            return process.ReferenceName ?? process.Name;
        }
    }
}
