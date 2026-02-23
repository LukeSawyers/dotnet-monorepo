using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DotnetMonorepo.SolutionGeneration;

public class ProjectSolutionGenerator(IOptionsMonitor<ProjectsSolutionGeneration[]> options) : BackgroundService
{
    internal static void GenerateSolutionFileForProjects(string solutionFilePath, string[] projectFilePaths)
    {
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}