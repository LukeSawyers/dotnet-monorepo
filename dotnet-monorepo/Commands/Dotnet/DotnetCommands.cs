using System.CommandLine;
using CliWrap;
using DotnetMonorepo.Commands.SolutionGeneration;
using DotnetMonorepo.Extensions;
using Microsoft.Build.Construction;
using Microsoft.Extensions.Logging;
using Command = System.CommandLine.Command;

namespace DotnetMonorepo.Commands.Dotnet;

public class DotnetCommands(
    ILogger<DotnetCommands> logger,
    SolutionGenerator solutionGenerator,
    CliCommandHelper commandHelper,
    Directories directories
) : ICommandBuilder
{
    private Option<string> AffectedPath { get; } = new("--repository-path")
    {
        Description = $"Specify the path to the git repository to run commands on. " +
                      $"By default the tool will try to find a git repository by traversing up from the current working directory. "
    };

    private Option<string> AffectedFrom { get; } = new("--from")
    {
        Description = "The ref to compare against --to"
    };

    private Option<string> AffectedTo { get; } = new("--to")
    {
        Description = "The ref to compare against --from"
    };

    private Command DotnetCommandWithAffected(
        string command
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

    private async Task DotnetCommandAsync(
        string command,
        string[] projects,
        ParseResult parseResult,
        CancellationToken cancellationToken
    )
    {
        var tempSolutionFile = Path.Combine(Directories.TempFolder, "solution.slnx");

        await solutionGenerator.GenerateSolutionAsync(
            projects,
            tempSolutionFile,
            cancellationToken
        );

        await commandHelper.ExecuteCommandAsync(
            Path.GetDirectoryName(tempSolutionFile)!,
            "dotnet",
            $"{command} {Path.GetFileName(tempSolutionFile)} {string.Join(' ', parseResult.UnmatchedTokens)}",
            cancellationToken: cancellationToken
        );
    }

    private async Task<string[]> GetAffectedProjectsAsync(
        ParseResult parseResult,
        CancellationToken ct
    )
    {
        var repositoryPath = parseResult.GetValue(AffectedPath) ??
                             directories.GetRepositoryRoot(Environment.CurrentDirectory);

        if (repositoryPath is null)
        {
            logger.LogError(
                $"Unable to find a git repository, run from an initialized git repository or specify with {AffectedPath.Name}"
            );

            return [];
        }

        var args =
            "tool exec dotnet-affected -y --allow-roll-forward --ignore-failed-sources --" +
            $" --repository-path {repositoryPath}" +
            $" --output-dir {Directories.TempFolder}" +
            parseResult.GetValue(AffectedFrom)?.Let(f => $" --from {f}") +
            parseResult.GetValue(AffectedTo)?.Let(t => $" --to {t}");

        await commandHelper.ExecuteCommandAsync(repositoryPath, "dotnet", args, CommandResultValidation.None, ct);

        var affectedFilePath = Path.Combine(Directories.TempFolder, "affected.proj");

        if (!File.Exists(affectedFilePath))
        {
            return [];
        }

        var projects = ProjectRootElement.Open(affectedFilePath)?.GetReferencedProjectPaths() ?? [];

        if (!projects.Any())
        {
            return projects;
        }

        logger.LogInformation("Affected:");
        foreach (var project in projects)
        {
            logger.LogDebug($" - {project}");
        }

        return projects;
    }

    void ICommandBuilder.Build(RootCommand rootCommand)
    {
        rootCommand.Add(new Command("affected")
        {
            AffectedPath,
            AffectedFrom,
            AffectedTo,
        }.Apply(c =>
        {
            c.TreatUnmatchedTokensAsErrors = false;
            c.SetAction(GetAffectedProjectsAsync);
        }));

        var dotnetCommands = new[]
        {
            "clean",
            "restore",
            "build",
            "pack",
            "publish",
            "test",
        };

        foreach (var dotnetCommand in dotnetCommands)
        {
            rootCommand.Add(DotnetCommandWithAffected(dotnetCommand));
        }
    }
}