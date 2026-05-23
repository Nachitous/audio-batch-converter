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

        // Drain both pipes on thread-pool threads. ReadToEnd blocks until the
        // process closes its handles (i.e. exits), which avoids the pipe-buffer
        // deadlock that ReadToEndAsync(ct) can hit when ct is cancelled mid-read.
        var stdoutTask = Task.Run(() => process.StandardOutput.ReadToEnd());
        var stderrTask = Task.Run(() => process.StandardError.ReadToEnd());
        await Task.WhenAll(stdoutTask, stderrTask).ConfigureAwait(false);

        return (process.ExitCode, stderrTask.Result);
    }
}
