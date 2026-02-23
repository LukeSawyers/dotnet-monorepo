using System.CommandLine;
using System.Runtime.CompilerServices;
using CliWrap;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;
using Command = System.CommandLine.Command;

namespace DotnetMonorepo.Commands;

public static class Commands
{
    private static Option<string> AffectedPath { get; } = new("--repository-path");
    private static Option<string> AffectedFrom { get; } = new("--from");
    private static Option<string> AffectedTo { get; } = new("--to");

    public static Command Affected { get; } = new Command("affected")
    {
        AffectedPath,
        AffectedFrom,
        AffectedTo,
    }.Apply(c =>
    {
        c.TreatUnmatchedTokensAsErrors = false;
        c.SetAction(async (result, token) => await GetAffectedProjectsAsync(result, token));
    });

    public static Command Build { get; } = DotnetCommandWithAffected();
    public static Command Restore { get; } = DotnetCommandWithAffected();
    public static Command Clean { get; } = DotnetCommandWithAffected();
    public static Command Pack { get; } = DotnetCommandWithAffected();
    public static Command Test { get; } = DotnetCommandWithAffected();

    private static Command DotnetCommandWithAffected(
        [CallerMemberName] string command = ""
    ) => new Command(command.ToLowerInvariant())
    {
        AffectedPath,
        AffectedFrom,
        AffectedTo,
    }.Apply(c =>
    {
        c.TreatUnmatchedTokensAsErrors = false;
        c.SetAction(async (result, token) =>
        {
            var projects = await GetAffectedProjectsAsync(result, token);
            await DotnetCommandAsync(command.ToLowerInvariant(), projects, result, token);
        });
    });
    
    private static async Task DotnetCommandAsync(
        string command,
        string[] projects,
        ParseResult parseResult,
        CancellationToken cancellationToken
    )
    {
        var solutionModel = new SolutionModel();
        foreach (var project in projects)
        {
            solutionModel.AddProject(project);
        }

        var tempSolutionFile = Path.Combine(Directories.TempFolder, "solution.slnx");
        await SolutionSerializers.SlnXml.SaveAsync(tempSolutionFile, solutionModel, cancellationToken);

        await BuildCommand(
            Path.GetDirectoryName(tempSolutionFile)!,
            "dotnet",
            $"{command} {Path.GetFileName(tempSolutionFile)} {string.Join(' ', parseResult.UnmatchedTokens)}"
        ).ExecuteAsync(cancellationToken);
    }

    private static async Task<string[]> GetAffectedProjectsAsync(
        ParseResult parseResult,
        CancellationToken ct
    )
    {
        var repositoryPath = parseResult.GetValue(AffectedPath) ?? Directories.RepositoryRoot;

        var args =
            "tool exec dotnet-affected --" +
            $" --repository-path {repositoryPath}" +
            $" --output-dir {Directories.TempFolder}" +
            parseResult.GetValue(AffectedFrom)?.Let(f => $" --from {f}") +
            parseResult.GetValue(AffectedTo)?.Let(t => $" --to {t}");

        await BuildCommand(repositoryPath, "dotnet", args)
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync(ct);

        var affectedFilePath = Path.Combine(Directories.TempFolder, "affected.proj");

        if (!File.Exists(affectedFilePath))
        {
            return [];
        }

        var root = ProjectRootElement.Open(affectedFilePath);

        var projects = root.ItemGroups
            .SelectMany(i => i.Items)
            .Where(i => i.ElementName == "ProjectReference")
            .Select(i => i.Include)
            .ToArray();

        if (!projects.Any())
        {
            return projects;
        }

        Console.WriteLine("Affected:");
        foreach (var project in projects)
        {
            Console.WriteLine($" - {project}");
        }

        return projects;
    }

    private static CliWrap.Command BuildCommand(
        string workingDirectory,
        string tool,
        string args
    )
    {
        Console.WriteLine($"{tool} {args} @ {workingDirectory}");

        return Cli.Wrap(tool)
            .WithArguments(args)
            .WithWorkingDirectory(workingDirectory)
            .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
            .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()));
    }
}