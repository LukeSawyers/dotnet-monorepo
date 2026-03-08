using CliWrap;
using Microsoft.Extensions.Logging;

namespace DotnetMonorepo;

public class CliCommandHelper(ILogger<CliCommandHelper> logger) : ICliCommandRunner
{
    public Task ExecuteCommandAsync(
        string workingDirectory,
        string tool,
        string args,
        CommandResultValidation validation = CommandResultValidation.ZeroExitCode,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation($"{tool} {args} @ {workingDirectory}");
        return Cli.Wrap(tool)
            .WithArguments(args)
            .WithWorkingDirectory(workingDirectory)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(line => logger.LogDebug(line)))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(line => logger.LogError(line)))
            .WithValidation(validation)
            .ExecuteAsync(cancellationToken);
    }
    
}