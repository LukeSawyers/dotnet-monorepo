namespace DotnetMonorepo.Tests.Commands.SolutionGeneration;

public class SolutionGeneratorTests
{
    private const string Project1 =
        //language=xml
        """
        <Project>
            <ItemGroup>
                <ProjectReference Include="..\Project2\Project2.csproj"/>
                <ProjectReference Include="..\Project5\Project5.csproj"/>
            </ItemGroup>
        </Project>
        """;
    
    private const string Project2 =
        //language=xml
        """
        <Project>
            <ItemGroup>
                <ProjectReference Include="..\Project3\Project3.csproj"/>
                <ProjectReference Include="..\Project3\Project4.csproj"/>
            </ItemGroup>
        </Project>
        """;
    
    private const string Project3 =
        //language=xml
        """
        <Project>
            <ItemGroup>
                <ProjectReference Include="..\Project4\Project4.csproj"/>
            </ItemGroup>
        </Project>
        """;
    
    private const string Project4 =
        //language=xml
        """
        <Project>
        </Project>
        """;
    
    private const string Project5 =
        //language=xml
        """
        <Project>
        </Project>
        """;

    [Fact]
    public void METHOD()
    {
        // Arrange
        
        
        // Act
        
        
        // Assert
        
        
    }
}