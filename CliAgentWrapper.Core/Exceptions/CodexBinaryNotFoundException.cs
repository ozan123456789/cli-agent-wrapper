namespace CliAgentWrapper.Core.Exceptions;

[Obsolete("Use CliBinaryNotFoundException for provider-agnostic error handling.")]
public sealed class CodexBinaryNotFoundException : CliBinaryNotFoundException
{
    public CodexBinaryNotFoundException(string executableName, Exception? innerException = null)
        : base(executableName, innerException)
    {
    }
}

