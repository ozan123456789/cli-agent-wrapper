namespace CliAgentWrapper.Core.Abstractions;

using CliAgentWrapper.Core.Models;

public interface IAgentProvider
{
    string Id { get; }
    string DisplayName { get; }

    ProcessInvocation BuildInvocation(AgentRequest request);
}

