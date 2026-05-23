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

        // Kill ffmpeg if cancellation is requested — otherwise a hung process
        // keeps the read tasks alive forever.
        using var kill = ct.Register(() =>
        {
            try { process.Kill(entireProcessTree: true); } catch { }
        });

        // Read stdout and stderr concurrently. Redirecting stdout but never
        // draining it causes a deadlock once the OS pipe buffer (~64 KB) fills.
        var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = process.StandardError.ReadToEndAsync(ct);
        await Task.WhenAll(stdoutTask, stderrTask);
        await process.WaitForExitAsync(ct);
        return (process.ExitCode, await stderrTask);
    }
}
