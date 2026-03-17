namespace CliAgentWrapper.Infrastructure;

using CliAgentWrapper.Core.Abstractions;
using CliAgentWrapper.Core.Models;
using CliAgentWrapper.Infrastructure.Providers;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCliAgentWrapper(this IServiceCollection services, Action<AgentOptions>? configure = null)
    {
        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.AddOptions<AgentOptions>();
        }

        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<IAgentProvider, CodexCliProvider>();
        services.AddSingleton<IAgentProvider, GeminiCliProvider>();
        services.AddSingleton<IAgentService, AgentService>();

        return services;
    }
}

