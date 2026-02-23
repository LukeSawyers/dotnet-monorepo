using Jig.Targets;

namespace build.Targets;

public class Workflows(
    DotnetTargets dotnet
) : ITargetProvider
{
    public ITarget Validate => field ??= new Target(description: "Runs all validation checks")
        .DependentOn(
            () => ValidateCode
        );
    
    public ITarget ValidateCode => field ??= new Target(description: "Validates code compiles and runs correctly")
        .DependentOn(
            () => dotnet.Build,
            () => dotnet.Test,
            () => dotnet.Pack
        );

    public ITarget Deploy => field ?? new Target(description: "Runs actions required to publish artifacts")
        .DependentOn(() => dotnet.NugetPush);
}