using System.CommandLine;
using DotnetMonorepo.Extensions;

namespace DotnetMonorepo.Commands.SolutionGeneration;

public class SolutionGenerationCommands(SolutionGenerator solutionGenerator) : ICommandBuilder
{
    public void Build(RootCommand rootCommand)
    {
        var projects = new Argument<string[]>("projects")
        {
            Description = "Project to generate a solution for",
            Arity = ArgumentArity.OneOrMore
        };

        var open = new Option<bool>("--open")
        {
            Aliases = { "-o" },
            Description = "Open the project once it's generated"
        };

        var generate = new Command("generate")
        {
            projects,
            open
        }.Apply(c =>
        {
            c.Aliases.Add("g");
            c.SetAction(async (parseResult, cancellationToken) => await solutionGenerator.GenerateSolutionAsync(
                parseResult.GetValue(projects) ?? [],
                parseResult.GetValue(open),
                cancellationToken
            ));
        });

        rootCommand.Add(generate);
    }
}