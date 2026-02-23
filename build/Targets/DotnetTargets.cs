using AwesomeAssertions;
using Jig.Options;
using Jig.Shell;
using Jig.Targets;
using MoreLinq;

namespace build.Targets;

public class DotnetTargets : ITargetProvider
{
    public BuildOption<string> Verbosity { get; } = new("minimal", description: "Verbosity for dotnet tasks");
    
    public BuildOption<string?> NugetApiKey { get; } = new(null, sensitive: true, description: "API key used to push nuget packages");

    // Build & Test
    public ITarget Build => field ??= new Target(description: "Builds the solution")
        .Executes($"dotnet build {BuildConstants.SolutionPath} --verbosity {Verbosity} --configuration Release");

    public ITarget Test => field ??= new Target(description: "Tests the solution")
        .After(() => Build)
        .Executes($"dotnet test {BuildConstants.SolutionPath} --verbosity {Verbosity} --configuration Release");

    // Packaging
    public ITarget ClearNugetPackages => field ??= new Target(description: "Clears nuget packages from the directory")
        .Before(() => Build)
        .ExecutesExpression(() => new DirectoryInfo(Directory.GetCurrentDirectory())
            .GetFiles("*.nupkg", SearchOption.AllDirectories)
            .ForEach(f => f.Delete()));

    public ITarget Pack => field ??= new Target(description: "Generates nuget packages")
        .DependentOn(() => ClearNugetPackages)
        .After(() => Test)
        .Executes($"dotnet pack {BuildConstants.SolutionPath} --verbosity {Verbosity} --configuration Release");

    public ITarget NugetPush => field ??= new Target(description: "Pushes nuget packages to nuget.org")
        .DependentOn(() => Pack)
        .ExecutesExpression(() => NugetApiKey.Value.Should().NotBeNull())
        .ExecutesDefaultShell(
            $"""
             dotnet nuget push **/*.nupkg 
             --api-key {NugetApiKey} 
             --skip-duplicate 
             --source https://api.nuget.org/v3/index.json
             """
        );
}