using Microsoft.Build.Construction;

namespace DotnetMonorepo.Extensions;

public static class ProjectExtensions
{
    extension(ProjectRootElement project)
    {
        public string [] GetReferencedProjectPaths() => project.ItemGroups
            .SelectMany(i => i.Items)
            .Where(i => i.ElementName == "ProjectReference")
            .Select(i => i.Include.Replace('\\', Path.DirectorySeparatorChar))
            .ToArray();
    }
}