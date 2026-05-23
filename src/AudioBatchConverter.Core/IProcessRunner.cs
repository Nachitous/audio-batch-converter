namespace AudioBatchConverter.Core;

public interface IProcessRunner
{
    Task<(int ExitCode, string StdErr)> RunAsync(
        string executable, string arguments, CancellationToken ct);
}
