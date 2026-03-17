namespace CliAgentWrapper.Core.Models;

public sealed record AgentRequest(
    string ProviderId,
    string Prompt,
    string? WorkingDirectory = null,
    IReadOnlyList<string>? Arguments = null,
    TimeSpan? Timeout = null,
    IReadOnlyDictionary<string, string?>? EnvironmentVariables = null,
    bool AllowInteractive = false);

