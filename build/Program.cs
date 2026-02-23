// See https://aka.ms/new-console-template for more information

using Jig.Build;
using Jig.DesktopNotifications;
using Jig.GitHubActions;
using Jig.Lang;
using Jig.Serilog;
using Jig.Shell;

return await new Build(defaultBuildConcurrency: BuildConcurrency.Parallel)
    .AddShell()
    .AddSerilog()
    .If(!GitHubActionsEnvironment.IsRunningGitHubActions, b => b.AddNotifications())
    .AddTargetsFromEntryAssembly()
    .ExecuteAsync(args);