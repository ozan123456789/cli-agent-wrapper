namespace CliAgentWrapper.Core.Abstractions;

using CliAgentWrapper.Core.Models;

public interface IAgentService
{
    Task<AgentResponse> ExecuteAsync(AgentRequest request, CancellationToken cancellationToken = default);
}

