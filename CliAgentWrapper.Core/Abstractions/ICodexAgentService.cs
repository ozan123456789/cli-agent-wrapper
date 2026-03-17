namespace CliAgentWrapper.Core.Abstractions;

using CliAgentWrapper.Core.Models;

public interface ICodexAgentService
{
    Task<AgentResponse> ExecuteAsync(AgentRequest request, CancellationToken cancellationToken = default);
}

