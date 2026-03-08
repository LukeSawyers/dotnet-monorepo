using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace DotnetMonorepo.Tests;

public class DirectoriesTests(ITestOutputHelper output)
{
    private readonly MockFileSystem _fileSystem = new();

    private readonly ILoggerFactory _loggerFactory = new LoggerFactory().AddXunit(output);

    [Fact]
    public void GetRepositoryRoot_WithParentFolderContainingGitDirectory_ReturnsParentFolder()
    {
        // Arrange
        var repositoryRoot = "/root/repo";
        var startingDirectory = Path.Combine(repositoryRoot, "src/codefolder");
        _fileSystem.AddDirectory(repositoryRoot);
        _fileSystem.AddDirectory(Path.Combine(repositoryRoot, ".git"));
        _fileSystem.AddDirectory(startingDirectory);
        var directories = new Directories(_loggerFactory.CreateLogger<Directories>(), _fileSystem);

        // Act
        var result = directories.GetRepositoryRoot(startingDirectory);

        // Assert
        result.Should().BeEquivalentTo(repositoryRoot);
    }
    
    [Fact]
    public void GetRepositoryRoot_WithNoParentGitFolder_ReturnsNull()
    {
        // Arrange
        var repositoryRoot = "/root/repo";
        var startingDirectory = Path.Combine(repositoryRoot, "src/codefolder");
        _fileSystem.AddDirectory(repositoryRoot);
        _fileSystem.AddDirectory(startingDirectory);
        var directories = new Directories(_loggerFactory.CreateLogger<Directories>(), _fileSystem);

        // Act
        var result = directories.GetRepositoryRoot(startingDirectory);

        // Assert
        result.Should().BeNull();
    }
}