namespace CliAgentWrapper.Infrastructure;

using CliAgentWrapper.Core.Abstractions;
using CliAgentWrapper.Core.Exceptions;
using CliAgentWrapper.Core.Models;
using Microsoft.Extensions.Logging;

public sealed class AgentService : IAgentService
{
    private readonly IReadOnlyDictionary<string, IAgentProvider> _providers;
    private readonly IProcessRunner _processRunner;
    private readonly ILogger<AgentService> _logger;

    public AgentService(IEnumerable<IAgentProvider> providers, IProcessRunner processRunner, ILogger<AgentService> logger)
    {
        _providers = providers.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
        _processRunner = processRunner;
        _logger = logger;
    }

    public async Task<AgentResponse> ExecuteAsync(AgentRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.ProviderId)) throw new ArgumentException("ProviderId is required.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Prompt)) throw new ArgumentException("Prompt is required.", nameof(request));

        if (!_providers.TryGetValue(request.ProviderId, out var provider))
        {
            var known = string.Join(", ", _providers.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
            throw new KeyNotFoundException($"Unknown provider '{request.ProviderId}'. Known: [{known}]");
        }

        var invocation = provider.BuildInvocation(request);

        ProcessExecutionResult result;
        try
        {
            result = await _processRunner.RunAsync(
                fileName: invocation.FileName,
                arguments: invocation.Arguments,
                workingDirectory: invocation.WorkingDirectory,
                environmentVariables: invocation.EnvironmentVariables,
                stdIn: invocation.StdIn,
                timeout: invocation.Timeout,
                allowInteractive: invocation.AllowInteractive,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is FileNotFoundException or System.ComponentModel.Win32Exception)
        {
            throw new CliBinaryNotFoundException(invocation.FileName, ex);
        }

        var succeeded = !result.TimedOut && result.ExitCode == 0;

        _logger.LogDebug(
            "Provider {ProviderId} finished. ExitCode={ExitCode}, TimedOut={TimedOut}, DurationMs={DurationMs}.",
            provider.Id,
            result.ExitCode,
            result.TimedOut,
            (int)result.Duration.TotalMilliseconds);

        return new AgentResponse(
            StdOut: result.StdOut,
            StdErr: result.StdErr,
            ExitCode: result.ExitCode,
            Succeeded: succeeded,
            TimedOut: result.TimedOut,
            Duration: result.Duration);
    }
}

