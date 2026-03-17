namespace CliAgentWrapper.Core.Models;

public sealed record AgentOptions
{
    public string CodexExecutable { get; set; } = "codex";
    public string GeminiExecutable { get; set; } = "gemini";
}

