namespace CliAgentWrapper.Core.Models;

public sealed record ProcessExecutionResult(
    string StdOut,
    string StdErr,
    int ExitCode,
    bool TimedOut,
    TimeSpan Duration);

