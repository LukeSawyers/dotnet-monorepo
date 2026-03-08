using System.IO.Abstractions;
using DotnetMonorepo.Extensions;
using GlobExpressions;
using Microsoft.Build.Construction;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;

namespace DotnetMonorepo.Commands.SolutionGeneration;

public class SolutionGenerator(
    ILogger<SolutionGenerator> logger,
    IFileSystem fileSystem,
    ICliCommandRunner commandHelper,
    Directories directories
)
{
    public async Task GenerateSolutionAsync(
        IEnumerable<string> projects,
        string solutionPath,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation($"Generating solution {solutionPath} with projects:");

        var solutionModel = new SolutionModel();
        foreach (var project in projects)
        {
            solutionModel.AddProject(project);
            logger.LogDebug($" - {project}");
        }

        await using var fileStream = fileSystem.File.Create(solutionPath);
        await SolutionSerializers.SlnXml.SaveAsync(fileStream, solutionModel, cancellationToken);
    }

    /// <summary>
    /// Generate a solution for a single project
    /// </summary>
    public async Task GenerateSolutionAsync(
        string[] projectPatterns,
        bool open,
        CancellationToken cancellationToken
    )
    {
        var resolvedProjects = ResolveProjectsFromArguments(projectPatterns);

        foreach (var resolvedProject in resolvedProjects)
        {
            var includedProjects = new HashSet<string> { resolvedProject };

            GetProjectDependentsRecursive(resolvedProject, includedProjects);

            GetProjectDependenciesRecursive(resolvedProject, includedProjects);

            var solutionFile = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(resolvedProject)!,
                resolvedProject.Replace(".csproj", ".g.slnx")
            ));

            await GenerateSolutionAsync(includedProjects, solutionFile, cancellationToken);

            if (open)
            {
                await OpenSolution(solutionFile, cancellationToken);
            }
        }
    }

    private string[] ResolveProjectsFromArguments(string[] projectsArguments)
    {
        IEnumerable<string> FindProjects(string pattern)
        {
            if (Path.IsPathRooted(pattern))
            {
                var root = Path.GetPathRoot(pattern)!;
                var unrootedPattern = pattern.Substring(root.Length);

                return Glob.Files(root, unrootedPattern)
                    .Select(path => Path.Combine(root, path));
            }

            return Glob.Files(Environment.CurrentDirectory, pattern)
                .Select(path => Path.Combine(Environment.CurrentDirectory, path));
        }

        return projectsArguments
            .SelectMany(FindProjects)
            .Where(projectFile => Path.GetExtension(projectFile) == ".csproj")
            .ToArray();
    }

    private void GetProjectDependentsRecursive(string currentProject, ICollection<string> results)
    {
        var projectRepositoryRoot = directories.GetRepositoryRoot(Path.GetDirectoryName(currentProject));

        if (projectRepositoryRoot is null)
        {
            return;
        }

        var dependentProjects = Glob.Files(projectRepositoryRoot, "**/*.csproj")
            .SelectMany(p =>
            {
                var projectPath = Path.GetFullPath(Path.Combine(projectRepositoryRoot, p));
                return ProjectRootElement.Open(projectPath)
                    ?.GetReferencedProjectPaths()
                    .Select(relativeReferencedProjectPath =>
                    {
                        var referencedProjectPath =
                            Path.GetFullPath(Path.Combine(Path.GetDirectoryName(currentProject)!,
                                relativeReferencedProjectPath));
                        return (projectPath, referencedProjectPath);
                    }) ?? [];
            })
            .GroupBy(pair => pair.referencedProjectPath, pair => pair.projectPath)
            .ToDictionary(g => g.Key, g => g.ToArray());

        GetProjectDependentsRecursive(currentProject, dependentProjects, results);
    }

    private void GetProjectDependentsRecursive(
        string currentProject,
        IReadOnlyDictionary<string, string[]> dependentProjects,
        ICollection<string> projects
    )
    {
        if (!dependentProjects.TryGetValue(currentProject, out var dependentProjectPaths))
        {
            return;
        }

        foreach (var dependentProjectPath in dependentProjectPaths)
        {
            projects.Add(dependentProjectPath);
            GetProjectDependentsRecursive(dependentProjectPath, dependentProjects, projects);
        }
    }

    private void GetProjectDependenciesRecursive(string currentProject, ICollection<string> results)
    {
        var dependencyProjects = ProjectRootElement.Open(currentProject)
            ?.GetReferencedProjectPaths()
            .Select(p => Path.Combine(Path.GetDirectoryName(currentProject)!, p)) ?? [];

        foreach (var project in dependencyProjects)
        {
            results.Add(project);
            GetProjectDependenciesRecursive(project, results);
        }
    }

    private Task OpenSolution(string solutionFilePath, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows())
        {
            return commandHelper.ExecuteCommandAsync(
                "open",
                solutionFilePath,
                cancellationToken: cancellationToken);
        }

        if (OperatingSystem.IsMacOS())
        {
            return commandHelper.ExecuteCommandAsync(
                "open",
                solutionFilePath,
                cancellationToken: cancellationToken
            );
        }

        if (OperatingSystem.IsLinux())
        {
            return commandHelper.ExecuteCommandAsync(
                "xdg-open",
                solutionFilePath,
                cancellationToken: cancellationToken
            );
        }

        logger.LogError("Unable to open solution file, operating system not supported");
        return Task.CompletedTask;
    }
}