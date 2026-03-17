# CLI Agent Wrapper (.NET 8)

This solution wraps local AI CLIs (e.g. `codex`, `gemini`) as subprocesses (stdin/stdout/stderr) with a clean, testable architecture.

## Projects

- `CliAgentWrapper.Core`: abstractions + request/response models
- `CliAgentWrapper.Infrastructure`: `System.Diagnostics.Process` implementation + service
- `CliAgentWrapper.ConsoleDemo`: simple console demo
- `CliAgentWrapper.Tests`: unit tests (no `codex` dependency)

## Run the demo

From the repo root:

```powershell
dotnet run --project .\CliAgentWrapper.ConsoleDemo\
```

If the CLIs are not on PATH, you can set executable paths via config (recommended) or env vars.

Config file:
- `CliAgentWrapper.ConsoleDemo/appsettings.json` → `AgentOptions:CodexExecutable`, `AgentOptions:GeminiExecutable`

```powershell
$env:CODEX_EXECUTABLE="C:\path\to\codex.exe"
$env:GEMINI_EXECUTABLE="C:\path\to\gemini.cmd"
dotnet run --project .\CliAgentWrapper.ConsoleDemo\
```

## Test

```powershell
dotnet test
```

