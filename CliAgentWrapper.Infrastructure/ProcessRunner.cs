namespace CliAgentWrapper.Infrastructure;

using System.ComponentModel;
using System.Diagnostics;
using CliAgentWrapper.Core.Abstractions;
using CliAgentWrapper.Core.Models;
using Microsoft.Extensions.Logging;

public sealed class ProcessRunner : IProcessRunner
{
    private readonly ILogger<ProcessRunner> _logger;

    public ProcessRunner(ILogger<ProcessRunner> logger)
    {
        _logger = logger;
    }

    public async Task<ProcessExecutionResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string? workingDirectory,
        IReadOnlyDictionary<string, string?>? environmentVariables,
        string? stdIn,
        TimeSpan? timeout,
        bool allowInteractive,
        CancellationToken cancellationToken)
    {
        var startedAt = Stopwatch.GetTimestamp();

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (timeout is not null && timeout.Value > TimeSpan.Zero)
        {
            linkedCts.CancelAfter(timeout.Value);
        }

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = false,
            RedirectStandardInput = !allowInteractive,
            RedirectStandardOutput = !allowInteractive,
            RedirectStandardError = !allowInteractive,
            CreateNoWindow = !allowInteractive,
        };

        if (!string.IsNullOrWhiteSpace(workingDirectory))
        {
            psi.WorkingDirectory = workingDirectory!;
        }

        foreach (var arg in arguments)
        {
            psi.ArgumentList.Add(arg);
        }

        if (environmentVariables is not null)
        {
            foreach (var (key, value) in environmentVariables)
            {
                // Never log secrets / env values. Only log keys at debug level.
                psi.Environment[key] = value;
            }
        }

        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

        try
        {
            _logger.LogDebug(
                "Starting process {FileName} with {ArgCount} args. WorkingDir={WorkingDirectory}. EnvKeys={EnvKeysCount}.",
                fileName,
                arguments.Count,
                psi.WorkingDirectory,
                environmentVariables?.Count ?? 0);

            if (!process.Start())
            {
                throw new InvalidOperationException($"Process failed to start: '{fileName}'.");
            }
        }
        catch (Exception ex) when (ex is Win32Exception or FileNotFoundException)
        {
            // Normalize "binary not found" scenarios for caller.
            throw;
        }

        Task<string> stdOutTask = allowInteractive
            ? Task.FromResult(string.Empty)
            : process.StandardOutput.ReadToEndAsync(linkedCts.Token);

        Task<string> stdErrTask = allowInteractive
            ? Task.FromResult(string.Empty)
            : process.StandardError.ReadToEndAsync(linkedCts.Token);

        Task writeStdInTask = Task.CompletedTask;
        if (!allowInteractive && !string.IsNullOrEmpty(stdIn))
        {
            writeStdInTask = WriteToStdInAsync(process, stdIn!, allowInteractive, linkedCts.Token);
        }
        else if (!allowInteractive)
        {
            // Non-interactive: close stdin to signal no more input.
            try { process.StandardInput.Close(); } catch { /* ignore */ }
        }

        var timedOut = false;
        try
        {
            await Task.WhenAll(writeStdInTask, process.WaitForExitAsync(linkedCts.Token)).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && timeout is not null)
        {
            timedOut = true;
        }
        catch (OperationCanceledException)
        {
            // external cancellation
        }

        if (!process.HasExited)
        {
            try
            {
                _logger.LogDebug("Killing process {FileName} (pid={Pid}).", fileName, process.Id);
                process.Kill(entireProcessTree: true);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to kill process {FileName}.", fileName);
            }
        }

        // Ensure streams drained. Avoid throwing here; best effort.
        string stdOut = string.Empty;
        string stdErr = string.Empty;
        try { stdOut = await stdOutTask.ConfigureAwait(false); } catch { }
        try { stdErr = await stdErrTask.ConfigureAwait(false); } catch { }

        var duration = Stopwatch.GetElapsedTime(startedAt);

        var exitCode = -1;
        try
        {
            if (process.HasExited)
            {
                exitCode = process.ExitCode;
            }
        }
        catch
        {
            // ignore
        }

        return new ProcessExecutionResult(
            StdOut: stdOut,
            StdErr: stdErr,
            ExitCode: exitCode,
            TimedOut: timedOut,
            Duration: duration);
    }

    private static async Task WriteToStdInAsync(Process process, string stdIn, bool allowInteractive, CancellationToken cancellationToken)
    {
        await process.StandardInput.WriteAsync(stdIn.AsMemory(), cancellationToken).ConfigureAwait(false);
        await process.StandardInput.FlushAsync(cancellationToken).ConfigureAwait(false);

        if (!allowInteractive)
        {
            // Signal end-of-input for non-interactive runs.
            try { process.StandardInput.Close(); } catch { /* ignore */ }
        }
    }
}

