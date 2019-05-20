using System;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi.Models;
using Microsoft.TeamFoundation.Core.WebApi;
using System.Collections.Generic;
using Newtonsoft.Json;
using AzdoBoardsManager.Models;
using AzdoBoardsManager.Exporter;
using CommandLine;

namespace Cse.AzDoBoardsManager
{
    class Program
    {
        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Option('d', "definitions-folder", Required = true, HelpText = "Definitions folder.")]
            public string DefinitionsFolder { get; set; }

            [Option('t', "cmd", Required = false, HelpText = "Specify the task to execute (export, import)")]
            public string Command { get; set; }

            [Option('u', "account-uri", Required = false, HelpText = "Account Uri for Azure DevOps")]
            public string AccountUri { get; set; }

            [Option('a', "access-token", Required = false, HelpText = "Access Token for Azure DevOps")]
            public string AccessToken { get; set; }
        }

        public static Dictionary<string, Guid> ProcessMap = new Dictionary<string, Guid>();
        public static Dictionary<string, WorkItemType> WorkItemTypes = new Dictionary<string, WorkItemType>();

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    if (o.Command.StartsWith("e"))
                    {
                        var connection = new VssConnection(
                            new Uri(o.AccountUri), new VssBasicCredential(string.Empty, o.AccessToken)
                        );

                        var projectsGenerator = new ProjectsGenerator()
                        {
                            BaseFolder = o.DefinitionsFolder
                        };
                        projectsGenerator.Generate(connection);

                        var processGenerator = new ProcessGenerator()
                        {
                            BaseFolder = o.DefinitionsFolder
                        };
                        processGenerator.Generate(connection);
                    } else if (o.Command.StartsWith("i"))
                    {
                        var connection = new VssConnection(new Uri(o.AccountUri), new VssBasicCredential(string.Empty, o.AccessToken));
                        
                        ReCreateEnvironment(o.DefinitionsFolder, connection);
                    }
                });
           
            
        }
        public static void ReCreateEnvironment(string folder, VssConnection connection)
        {
            var process = new List<string>() {
                "project.*.json",
                "process.*.json",
                "workitemtype.*.json",
                "workitemtypestates.*.json",
            };

            var processClient = connection.GetClient<WorkItemTrackingProcessHttpClient>();

            var existingProcesses = processClient.GetListOfProcessesAsync().SyncResult();
            foreach (var createdProcess in existingProcesses)
            {
                ProcessMap.Add(createdProcess.ReferenceName ?? createdProcess.Name, createdProcess.TypeId);
            } 

            foreach (var pattern in process)
            {
                foreach (var file in System.IO.Directory.GetFiles(folder, pattern))
                {
                    var fileContent = System.IO.File.ReadAllText(file);
                    var baseFileName = file.Replace(folder + @"\", "");
                    var type = baseFileName.Split('.')[0];
                    try
                    {
                        ProcessFile(connection, type, fileContent);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"-- {baseFileName} --");
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        public static void ProcessFile(VssConnection connection, string type, string fileContent)
        {
            switch(type)
            {
                case "project":
                    var project = JsonConvert.DeserializeObject<Project>(fileContent);
                    var projectClient = connection.GetClient<ProjectHttpClient>();

                    try
                    {
                        var existingProject = projectClient.GetProject(project.Name).SyncResult();

                        if (existingProject != null)
                        {
                            //TODO: add capabilities or changes
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        //this throws an exception if the project does not exists
                    }
                    

                    var capabilities = new Dictionary<string, Dictionary<string, string>>();

                    Dictionary<string, string> versionControlProperties = new Dictionary<string, string>();

                    versionControlProperties[TeamProjectCapabilitiesConstants.VersionControlCapabilityAttributeName] =
                        SourceControlTypes.Git.ToString();

                    // Setup process properties       
                    Dictionary<string, string> processProperaties = new Dictionary<string, string>();

                    processProperaties[TeamProjectCapabilitiesConstants.ProcessTemplateCapabilityTemplateTypeIdAttributeName] =
                        ProcessMap["Basic"].ToString();

                    capabilities[TeamProjectCapabilitiesConstants.VersionControlCapabilityName] = 
                        versionControlProperties;
                    capabilities[TeamProjectCapabilitiesConstants.ProcessTemplateCapabilityName] =
                      processProperaties;
                    var newProject = new TeamProject()
                    {
                        Name = project.Name,
                        Description = project.Description,
                        Visibility = (ProjectVisibility)project.Visibility,
                        Capabilities = capabilities
                    };
                    projectClient.QueueCreateProject(newProject).SyncResult();
                    break;
                case "process":
                    var process = JsonConvert.DeserializeObject<AzdoBoardsManager.Models.Process>(fileContent);
                    var processClient = connection.GetClient<WorkItemTrackingProcessHttpClient>();

                    if (ProcessMap.ContainsKey(process.ReferenceName ?? process.Name))
                    {
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(process.ReferenceName))
                    {
                        return;
                    }

                    var createProcess = new CreateProcessModel()
                    {
                        Name = process.Name,
                        Description = process.Description,
                        ReferenceName = process.ReferenceName,
                    };

                    if (!string.IsNullOrEmpty(process.ParentProcessId))
                    {
                        createProcess.ParentProcessTypeId = ProcessMap[process.ParentProcessId];
                    }

                    var processInfo = processClient.CreateNewProcessAsync(createProcess).SyncResult();
                    ProcessMap.Add(processInfo.ReferenceName ?? processInfo.Name, processInfo.TypeId);
                    break;
                case "workitemtype":
                    var workItemType = JsonConvert.DeserializeObject<AzdoBoardsManager.Models.WorkItemType>(fileContent);

                    WorkItemTypes.Add(workItemType.Id, workItemType);

                    var witClient = connection.GetClient<WorkItemTrackingProcessHttpClient>();

                    try
                    {
                        var types = witClient.GetProcessWorkItemTypesAsync(ProcessMap[workItemType.ProcessId]).SyncResult();
                        var existingWit = witClient.GetProcessWorkItemTypeAsync(ProcessMap[workItemType.ProcessId], workItemType.Id).SyncResult();

                        if (existingWit != null)
                        {
                            //TODO: add capabilities or changes
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        //this throws an exception if the project does not exists
                    }

                    var createWorkItemType = new CreateProcessWorkItemTypeRequest()
                    {
                        Name = workItemType.Name,
                        Description = workItemType.Description,
                        Color = workItemType.Color,
                        Icon = workItemType.Icon,
                        InheritsFrom = workItemType.Inherits,
                        IsDisabled = workItemType.IsDisabled
                    };
                    var processName = workItemType.ProcessId;
                    
                    witClient.CreateProcessWorkItemTypeAsync(createWorkItemType, ProcessMap[processName]).SyncResult();
                    break;
                case "workitemtypestates":
                    var witTrackingClient = connection.GetClient<WorkItemTrackingProcessHttpClient>();
                    var workItemTypeStates = JsonConvert.DeserializeObject<List<WorkItemTypeState>>(fileContent);

                    if (workItemTypeStates.Count == 0)
                    {
                        return;
                    }

                    var firstWorkItem = workItemTypeStates[0];
                    var workItemId = firstWorkItem.WorkItemId;
                    var processId = ProcessMap[WorkItemTypes[firstWorkItem.WorkItemId].ProcessId];
                    var stateDefinitions = witTrackingClient.GetStateDefinitionsAsync(
                            processId,
                            firstWorkItem.WorkItemId
                        ).SyncResult();
                    var stateDefinitionsMap = new Dictionary<string, bool>();

                    //We iterate to verify what is existing what needs to be created
                    foreach (var workItemTypeState in workItemTypeStates)
                    {
                        stateDefinitionsMap.Add(workItemTypeState.Name, false);

                        foreach (var def in stateDefinitions)
                        {
                            if (def.Name.Equals(workItemTypeState.Name))
                            {
                                stateDefinitionsMap[workItemTypeState.Name] = true;
                            }
                        }
                    }

                    //Extra will be deleted
                    foreach (var def in stateDefinitions)
                    {
                        if (!stateDefinitionsMap.ContainsKey(def.Name))
                        {
                            witTrackingClient.DeleteStateDefinitionAsync(
                                processId,
                                workItemId,
                                def.Id
                            ).SyncResult();
                        }
                    }

                    //We create the missing ones
                    foreach (var workItemTypeState in workItemTypeStates)
                    { 
                        if (!stateDefinitionsMap[workItemTypeState.Name])
                        {
                            var workItemStateInputModel = new WorkItemStateInputModel()
                            {
                                Name = workItemTypeState.Name,
                                Color = workItemTypeState.Color,
                                StateCategory = workItemTypeState.StateCategory,
                                Order = workItemTypeState.Order
                            };

                            var states = witTrackingClient.CreateStateDefinitionAsync(
                                workItemStateInputModel,
                                processId,
                                workItemTypeState.WorkItemId
                            ).SyncResult();
                        }
                    }
                    break;
            }
        }
    }
}


