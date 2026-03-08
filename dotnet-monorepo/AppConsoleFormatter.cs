using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

public class AppConsoleFormatter() : ConsoleFormatter(nameof(AppConsoleFormatter))
{
    public override void Write<TState>(
        in LogEntry<TState> logEntry, 
        IExternalScopeProvider? scopeProvider, 
        TextWriter textWriter
    )
    {
        var logLevelStr = logEntry.LogLevel switch
        {
            LogLevel.Warning => "Warning: ",
            LogLevel.Error => "Error: ",
            LogLevel.Critical => "Critical: ",
            _ => string.Empty
        };
        var message = logEntry.Formatter(logEntry.State, logEntry.Exception);
        textWriter.Write($"{logLevelStr}{message}{Environment.NewLine}");
    }
}