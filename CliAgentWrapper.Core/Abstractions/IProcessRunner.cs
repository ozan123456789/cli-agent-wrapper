namespace CliAgentWrapper.Core.Abstractions;

using CliAgentWrapper.Core.Models;

public interface IProcessRunner
{
    Task<ProcessExecutionResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string? workingDirectory,
        IReadOnlyDictionary<string, string?>? environmentVariables,
        string? stdIn,
        TimeSpan? timeout,
        bool allowInteractive,
        CancellationToken cancellationToken);
}

