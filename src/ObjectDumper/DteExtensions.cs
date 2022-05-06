using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Project = EnvDTE.Project;
using ProjectItem = EnvDTE.ProjectItem;

namespace ObjectDumper
{
    public static class DteExtensions
    {

        public static IEnumerable<Project> GetProjectsRecursively(this Solution solution)
        {
            return solution.Projects.OfType<Project>().SelectMany(GetProjects);
        }

        private static IEnumerable<Project> GetProjects(this Project project)
        {
            if (!project.IsSolutionFolder())
            {
                yield return project;
            }

            var containerProjects = new Queue<Project>();
            containerProjects.Enqueue(project);

            while (containerProjects.Any())
            {
                var containerProject = containerProjects.Dequeue();
                foreach (ProjectItem item in containerProject.ProjectItems)
                {
                    var nestedProject = item.SubProject;
                    if (nestedProject == null)
                    {
                        continue;
                    }

                    if (nestedProject.IsSolutionFolder())
                    {
                        containerProjects.Enqueue(nestedProject);
                    }
                    else
                    {
                        yield return nestedProject;
                    }
                }
            }
        }

        public static bool IsSolutionFolder(this Project project)
        {

            var solutionFolder = "2150E333-8FDC-42A3-9474-1A3956D46DE8";
            return project.Kind != null && project.Kind.Equals(solutionFolder, StringComparison.OrdinalIgnoreCase);
        }
    }
}