using System.IO.Abstractions;
using DotnetMonorepo.Extensions;
using Microsoft.Extensions.Logging;

namespace DotnetMonorepo;

public class Directories(ILogger<Directories> logger, IFileSystem fileSystem)
{
    public static string TempFolder { get; } = Path.Combine(
        Path.GetTempPath(),
        "dotnet-monorepo",
        Guid.NewGuid().ToString()
    ).Apply(d => Directory.CreateDirectory(d));
    
    /// <summary>
    /// Tries to find a git repository root from the supplied starting directory
    /// </summary>
    /// <param name="startingDirectory"></param>
    /// <returns></returns>
    public string? GetRepositoryRoot(string? startingDirectory)
    {
        if (startingDirectory is null)
        {
            return null;
        }
        
        var rootDirectory = fileSystem.DirectoryInfo.New(startingDirectory);
        
        while (true)
        {
            if (rootDirectory is null)
            {
                logger.LogWarning($"Unable to find repository root starting from {startingDirectory}");
                return null;
            }

            if (rootDirectory.EnumerateDirectories().Any(d => d.Name == ".git"))
            {
                break;
            }

            rootDirectory = rootDirectory.Parent;
        }

        return rootDirectory.FullName;
    }
}