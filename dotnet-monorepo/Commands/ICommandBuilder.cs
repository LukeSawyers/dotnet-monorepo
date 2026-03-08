using System.CommandLine;

namespace DotnetMonorepo.Commands;

public interface ICommandBuilder
{
    void Build(RootCommand rootCommand);
}