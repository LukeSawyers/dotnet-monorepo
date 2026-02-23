namespace DotnetMonorepo.SolutionGeneration;

/// <summary>
/// Configuration for monorepo solution generation
/// </summary>
/// <param name="Projects">
/// Projects to generate bespoke solution files for, encompassing all reachable dependent and dependency projects for that project. 
/// </param>
public record SolutionGeneration(
    ProjectsSolutionGeneration Projects
)
{
    public SolutionGeneration() : this(new ProjectsSolutionGeneration())
    {
    }
}