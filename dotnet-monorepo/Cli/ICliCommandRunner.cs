using CliWrap;

namespace DotnetMonorepo;

public interface ICliCommandRunner
{
    Task ExecuteCommandAsync(
        string tool,
        string args,
        CommandResultValidation validation = CommandResultValidation.ZeroExitCode,
        CancellationToken cancellationToken = default
    ) => ExecuteCommandAsync(Directory.GetCurrentDirectory(), tool, args, validation, cancellationToken);

    Task ExecuteCommandAsync(
        string workingDirectory,
        string tool,
        string args,
        CommandResultValidation validation = CommandResultValidation.ZeroExitCode,
        CancellationToken cancellationToken = default
    );
}