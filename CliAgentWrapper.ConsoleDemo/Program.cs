using CliAgentWrapper.Core.Abstractions;
using CliAgentWrapper.Core.Exceptions;
using CliAgentWrapper.Core.Models;
using CliAgentWrapper.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddCliAgentWrapper();
        services.Configure<AgentOptions>(context.Configuration.GetSection("AgentOptions"));
    })
    .Build();

var agent = host.Services.GetRequiredService<IAgentService>();
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ConsoleDemo");

Console.WriteLine("CLI Agent Wrapper demo");
Console.WriteLine("Select provider: [codex] or [gemini]. Press Enter for codex.");
var providerId = (Console.ReadLine() ?? string.Empty).Trim();
if (string.IsNullOrWhiteSpace(providerId))
{
    providerId = "codex";
}

Console.WriteLine("Enter prompt. Finish with an empty line.");
Console.WriteLine();

var lines = new List<string>();
while (true)
{
    var line = Console.ReadLine();
    if (line is null) break;
    if (string.IsNullOrEmpty(line)) break;
    lines.Add(line);
}

var prompt = string.Join(Environment.NewLine, lines);
if (string.IsNullOrWhiteSpace(prompt))
{
    Console.WriteLine("No prompt provided. Exiting.");
    return;
}

Console.WriteLine();
Console.WriteLine("Optional: enter extra Codex CLI arguments (single line). Leave empty for none.");
var argLine = Console.ReadLine() ?? string.Empty;
var extraArgs = string.IsNullOrWhiteSpace(argLine)
    ? Array.Empty<string>()
    : argLine.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

var request = new AgentRequest(
    ProviderId: providerId,
    Prompt: prompt,
    WorkingDirectory: Environment.CurrentDirectory,
    Arguments: extraArgs,
    Timeout: TimeSpan.FromMinutes(2),
    EnvironmentVariables: null,
    AllowInteractive: false);

try
{
    var response = await agent.ExecuteAsync(request);

    Console.WriteLine();
    Console.WriteLine("=== STDOUT ===");
    Console.WriteLine(response.StdOut);
    Console.WriteLine("=== STDERR ===");
    Console.WriteLine(response.StdErr);
    Console.WriteLine($"ExitCode: {response.ExitCode}  Succeeded: {response.Succeeded}  TimedOut: {response.TimedOut}  Duration: {response.Duration}");
}
catch (CliBinaryNotFoundException ex)
{
    logger.LogError(ex, "CLI binary could not be started.");
    Console.WriteLine(ex.Message);
}
