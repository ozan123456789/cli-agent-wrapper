namespace CliAgentWrapper.Core.Models;

public sealed record ProcessInvocation(
    string FileName,
    IReadOnlyList<string> Arguments,
    string? WorkingDirectory,
    IReadOnlyDictionary<string, string?>? EnvironmentVariables,
    string? StdIn,
    TimeSpan? Timeout,
    bool AllowInteractive);

