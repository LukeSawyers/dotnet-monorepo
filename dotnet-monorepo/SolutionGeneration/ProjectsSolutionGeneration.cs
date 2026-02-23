namespace DotnetMonorepo.SolutionGeneration;

/// <summary>
/// 
/// </summary>
/// <param name="Include">Projects to generate solutions for. Supports globbing.</param>
/// <param name="Exclude">Projects to not generate solutions for. Supports globbing.</param>
public record ProjectsSolutionGeneration(
    string[] Include,
    string[] Exclude
)
{
    public ProjectsSolutionGeneration() : this(["**"], [])
    {
        
    }
}