using System.CommandLine;
using System.IO.Abstractions;
using DotnetMonorepo;
using DotnetMonorepo.Commands;
using DotnetMonorepo.Commands.Dotnet;
using DotnetMonorepo.Commands.SolutionGeneration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings()
{
    Args = args,
});

builder.Logging
    .AddConsole(o => o.FormatterName = nameof(AppConsoleFormatter))
    .AddConsoleFormatter<AppConsoleFormatter, ConsoleFormatterOptions>()
    .SetMinimumLevel(LogLevel.Debug)
    .AddFilter("Microsoft*", _ => false);

builder.Services.AddSingleton<IFileSystem, FileSystem>();
builder.Services.AddSingleton<ICliCommandRunner, CliCommandHelper>();
builder.Services.AddSingleton<Directories>();
builder.Services.AddSingleton<SolutionGenerator>();

builder.Services.AddSingleton<ICommandBuilder, DotnetCommands>();
builder.Services.AddSingleton<ICommandBuilder, SolutionGenerationCommands>();

using var host = builder.Build();

await host.StartAsync();

var rootCommand = new RootCommand("Monorepo commands for dotnet cli");

foreach (var commandBuilder in host.Services.GetServices<ICommandBuilder>())
{
    commandBuilder.Build(rootCommand);
}

var cancellationToken = host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;
return await rootCommand.Parse(args).InvokeAsync(cancellationToken: cancellationToken);