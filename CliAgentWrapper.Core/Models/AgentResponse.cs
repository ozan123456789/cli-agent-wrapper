namespace CliAgentWrapper.Core.Models;

public sealed record AgentResponse(
    string StdOut,
    string StdErr,
    int ExitCode,
    bool Succeeded,
    bool TimedOut,
    TimeSpan Duration);

