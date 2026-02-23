using System.CommandLine;
using DotnetMonorepo.DotnetCommands;

var rootCommand = new RootCommand("Monorepo commands for dotnet cli")
{
    Commands.Affected,
    Commands.Clean,
    Commands.Restore,
    Commands.Build,
    Commands.Pack,
    Commands.Publish,
    Commands.Test,
};

await rootCommand.Parse(args).InvokeAsync();