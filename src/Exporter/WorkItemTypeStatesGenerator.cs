using Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AzdoBoardsManager.Exporter
{
    public class WorkItemTypeStatesGenerator : Base
    {
        public string WorkItemTypeName { get; set; }

        private ProcessInfo _process;

        public WorkItemTypeStatesGenerator(ProcessInfo process, string workItemTypeName)
        {
            _process = process;
            WorkItemTypeName = workItemTypeName;
        }

        public override void Generate(VssConnection connection)
        {
            var witClient = connection.GetClient<WorkItemTrackingProcessHttpClient>();
            
            var states = witClient.GetStateDefinitionsAsync(_process.TypeId, WorkItemTypeName).SyncResult(); //workItemType expand states

            if (!System.IO.Directory.Exists(BaseFolder))
            {
                System.IO.Directory.CreateDirectory(BaseFolder);
            }

            var statesDefinition = new List<Models.WorkItemTypeState>();

            foreach (var state in states)
            {
                var definition = ConvertObject<Models.WorkItemTypeState>(state);
                statesDefinition.Add(definition);
            }

            var definitionString = JsonConvert.SerializeObject(statesDefinition, Formatting.Indented);
            var fileName = string.Concat("workitemtypestates.", WorkItemTypeName.Replace('\\', '_').Replace('/', '_'), ".json");
            System.IO.File.WriteAllText(System.IO.Path.Combine(BaseFolder, fileName), definitionString);
        }

        public override void ProcessDefinition<T>(T definition)
        {
            var state = definition as Models.WorkItemTypeState;
            state.WorkItemId = WorkItemTypeName;
        }
    }
}
