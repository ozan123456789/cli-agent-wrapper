namespace CliAgentWrapper.Infrastructure.Providers;

using CliAgentWrapper.Core.Abstractions;
using CliAgentWrapper.Core.Models;
using Microsoft.Extensions.Options;

public sealed class CodexCliProvider : IAgentProvider
{
    private readonly AgentOptions _options;

    public CodexCliProvider(IOptions<AgentOptions> options)
    {
        _options = options.Value ?? new AgentOptions();
    }

    public string Id => "codex";
    public string DisplayName => "Codex CLI";

    public ProcessInvocation BuildInvocation(AgentRequest request)
    {
        var args = request.Arguments ?? Array.Empty<string>();
        var finalArgs = request.AllowInteractive
            ? args
            : BuildNonInteractiveArgs(args, request.Prompt, request.WorkingDirectory);

        return new ProcessInvocation(
            FileName: _options.CodexExecutable,
            Arguments: finalArgs,
            WorkingDirectory: request.WorkingDirectory,
            EnvironmentVariables: request.EnvironmentVariables,
            StdIn: null,
            Timeout: request.Timeout,
            AllowInteractive: request.AllowInteractive);
    }

    private static IReadOnlyList<string> BuildNonInteractiveArgs(IReadOnlyList<string> extraArgs, string prompt, string? workingDirectory)
    {
        var list = new List<string>(capacity: 6 + extraArgs.Count)
        {
            "exec",
        };

        if (!ContainsArg(extraArgs, "--skip-git-repo-check"))
        {
            list.Add("--skip-git-repo-check");
        }

        if (!string.IsNullOrWhiteSpace(workingDirectory) && !ContainsArg(extraArgs, "--cd") && !ContainsArg(extraArgs, "-C"))
        {
            list.Add("--cd");
            list.Add(workingDirectory!);
        }

        foreach (var a in extraArgs)
        {
            list.Add(a);
        }

        list.Add(prompt);
        return list;
    }

    private static bool ContainsArg(IReadOnlyList<string> args, string value)
    {
        foreach (var a in args)
        {
            if (string.Equals(a, value, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

