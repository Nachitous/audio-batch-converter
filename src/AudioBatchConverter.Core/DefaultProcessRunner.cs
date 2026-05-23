using System.Diagnostics;

namespace AudioBatchConverter.Core;

public class DefaultProcessRunner : IProcessRunner
{
    public async Task<(int ExitCode, string StdErr)> RunAsync(
        string executable, string arguments, CancellationToken ct)
    {
        var psi = new ProcessStartInfo(executable, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetTempPath(),
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException($"Failed to start process: {executable}");

        var stderr = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);
        return (process.ExitCode, stderr);
    }
}
