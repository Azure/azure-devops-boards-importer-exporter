using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzdoBoardsManager.Exporter
{
    public class ProjectsGenerator : Base
    {
        public ProjectsGenerator()
        {

        }

        public override void Generate(VssConnection connection)
        {
            var projectClient = connection.GetClient<ProjectHttpClient>();

            // Get a single page of projects
            IEnumerable<TeamProjectReference> projects = projectClient.GetProjects(top: 100).Result;

            // Add the set to the full list
            // Iterate and show the name of each project
            foreach (var project in projects)
            {
                var fullProject = projectClient.GetProject(project.Name, includeCapabilities: true);

                SaveObject<Models.Project>(project);
            }
        }
    }
}
