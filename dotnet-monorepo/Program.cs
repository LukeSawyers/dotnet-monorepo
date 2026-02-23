using System.CommandLine;
using DotnetMonorepo.Commands;

var rootCommand = new RootCommand("Monorepo commands for dotnet cli")
{
    Commands.Affected,
    Commands.Build,
    Commands.Restore,
    Commands.Clean,
    Commands.Pack,
    Commands.Test,
};

await rootCommand.Parse(args).InvokeAsync();