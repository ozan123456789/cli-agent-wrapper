namespace CliAgentWrapper.Tests;

using CliAgentWrapper.Core.Abstractions;
using CliAgentWrapper.Core.Models;
using CliAgentWrapper.Infrastructure;
using CliAgentWrapper.Infrastructure.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

public sealed class AgentServiceTests
{
    [Fact]
    public async Task ExecuteAsync_Maps_Process_Result_To_Response()
    {
        var runner = new FakeProcessRunner
        {
            NextResult = new ProcessExecutionResult(
                StdOut: "out",
                StdErr: "err",
                ExitCode: 0,
                TimedOut: false,
                Duration: TimeSpan.FromMilliseconds(123))
        };

        var providers = new IAgentProvider[]
        {
            new CodexCliProvider(Options.Create(new AgentOptions { CodexExecutable = "codex" })),
        };

        var service = new AgentService(
            providers,
            runner,
            NullLogger<AgentService>.Instance);

        var resp = await service.ExecuteAsync(new AgentRequest(ProviderId: "codex", Prompt: "hello", Arguments: new[] { "--foo", "bar" }));

        Assert.True(resp.Succeeded);
        Assert.False(resp.TimedOut);
        Assert.Equal(0, resp.ExitCode);
        Assert.Equal("out", resp.StdOut);
        Assert.Equal("err", resp.StdErr);
        Assert.Equal(TimeSpan.FromMilliseconds(123), resp.Duration);
    }

    [Fact]
    public async Task ExecuteAsync_Passes_Arguments_And_WorkingDirectory_To_ProcessRunner()
    {
        var runner = new FakeProcessRunner
        {
            NextResult = new ProcessExecutionResult(
                StdOut: "",
                StdErr: "",
                ExitCode: 0,
                TimedOut: false,
                Duration: TimeSpan.Zero)
        };

        var providers = new IAgentProvider[]
        {
            new CodexCliProvider(Options.Create(new AgentOptions { CodexExecutable = "codex" })),
        };

        var service = new AgentService(
            providers,
            runner,
            NullLogger<AgentService>.Instance);

        var req = new AgentRequest(
            ProviderId: "codex",
            Prompt: "p",
            WorkingDirectory: "C:\\work",
            Arguments: new[] { "--a", "b" },
            Timeout: TimeSpan.FromSeconds(7),
            EnvironmentVariables: new Dictionary<string, string?> { ["X"] = "1" },
            AllowInteractive: false);

        _ = await service.ExecuteAsync(req);

        Assert.Equal("codex", runner.LastFileName);
        Assert.Equal(req.WorkingDirectory, runner.LastWorkingDirectory);
        Assert.Equal(new[] { "exec", "--skip-git-repo-check", "--cd", "C:\\work", "--a", "b", "p" }, runner.LastArguments);
        Assert.Null(runner.LastStdIn);
        Assert.Equal(req.Timeout, runner.LastTimeout);
        Assert.Equal(req.AllowInteractive, runner.LastAllowInteractive);
        Assert.NotNull(runner.LastEnvironmentVariables);
        Assert.Equal("1", runner.LastEnvironmentVariables!["X"]);
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        public ProcessExecutionResult NextResult { get; set; } = new("", "", 0, false, TimeSpan.Zero);

        public string? LastFileName { get; private set; }
        public IReadOnlyList<string>? LastArguments { get; private set; }
        public string? LastWorkingDirectory { get; private set; }
        public IReadOnlyDictionary<string, string?>? LastEnvironmentVariables { get; private set; }
        public string? LastStdIn { get; private set; }
        public TimeSpan? LastTimeout { get; private set; }
        public bool LastAllowInteractive { get; private set; }

        public Task<ProcessExecutionResult> RunAsync(
            string fileName,
            IReadOnlyList<string> arguments,
            string? workingDirectory,
            IReadOnlyDictionary<string, string?>? environmentVariables,
            string? stdIn,
            TimeSpan? timeout,
            bool allowInteractive,
            CancellationToken cancellationToken)
        {
            LastFileName = fileName;
            LastArguments = arguments;
            LastWorkingDirectory = workingDirectory;
            LastEnvironmentVariables = environmentVariables;
            LastStdIn = stdIn;
            LastTimeout = timeout;
            LastAllowInteractive = allowInteractive;

            return Task.FromResult(NextResult);
        }
    }
}

