namespace CliAgentWrapper.Core.Exceptions;

public class CliBinaryNotFoundException : FileNotFoundException
{
    public CliBinaryNotFoundException(string executableName, Exception? innerException = null)
        : base($"CLI binary could not be found or started: '{executableName}'. Ensure it is installed and available on PATH (or configure the executable path).", innerException)
    {
        ExecutableName = executableName;
    }

    public string ExecutableName { get; }
}

