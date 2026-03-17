namespace CliAgentWrapper.Infrastructure.Providers;

using CliAgentWrapper.Core.Abstractions;
using CliAgentWrapper.Core.Models;
using Microsoft.Extensions.Options;

public sealed class GeminiCliProvider : IAgentProvider
{
    private readonly AgentOptions _options;

    public GeminiCliProvider(IOptions<AgentOptions> options)
    {
        _options = options.Value ?? new AgentOptions();
    }

    public string Id => "gemini";
    public string DisplayName => "Gemini CLI";

    public ProcessInvocation BuildInvocation(AgentRequest request)
    {
        var args = request.Arguments ?? Array.Empty<string>();

        // Gemini defaults to interactive; non-interactive uses -p/--prompt.
        var finalArgs = request.AllowInteractive
            ? args.Concat(new[] { request.Prompt }).ToArray()
            : BuildNonInteractiveArgs(args, request.Prompt);

        return new ProcessInvocation(
            FileName: _options.GeminiExecutable,
            Arguments: finalArgs,
            WorkingDirectory: request.WorkingDirectory,
            EnvironmentVariables: request.EnvironmentVariables,
            StdIn: null,
            Timeout: request.Timeout,
            AllowInteractive: request.AllowInteractive);
    }

    private static IReadOnlyList<string> BuildNonInteractiveArgs(IReadOnlyList<string> extraArgs, string prompt)
    {
        var list = new List<string>(capacity: 3 + extraArgs.Count)
        {
            "--prompt",
            prompt,
        };

        foreach (var a in extraArgs)
        {
            list.Add(a);
        }

        return list;
    }
}

