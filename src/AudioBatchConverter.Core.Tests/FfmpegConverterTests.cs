using AudioBatchConverter.Core;
using Xunit;

namespace AudioBatchConverter.Core.Tests;

public class FfmpegConverterTests
{
    private class FakeRunner : IProcessRunner
    {
        private readonly int _exitCode;
        private readonly string _stderr;

        public string? LastExecutable { get; private set; }
        public string? LastArguments { get; private set; }

        public FakeRunner(int exitCode = 0, string stderr = "")
        {
            _exitCode = exitCode;
            _stderr = stderr;
        }

        public Task<(int ExitCode, string StdErr)> RunAsync(
            string executable, string arguments, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            LastExecutable = executable;
            LastArguments = arguments;
            return Task.FromResult((_exitCode, _stderr));
        }
    }

    [Fact]
    public async Task ConvertAsync_SuccessExitCode_ReturnsSucceeded()
    {
        var runner = new FakeRunner(exitCode: 0);
        var converter = new FfmpegConverter("ffmpeg", runner);

        var result = await converter.ConvertAsync(@"C:\music\song.flac");

        Assert.True(result.Succeeded);
        Assert.Null(result.ErrorOutput);
    }

    [Fact]
    public async Task ConvertAsync_NonZeroExitCode_ReturnsFailure()
    {
        var runner = new FakeRunner(exitCode: 1, stderr: "no such file");
        var converter = new FfmpegConverter("ffmpeg", runner);

        var result = await converter.ConvertAsync(@"C:\music\song.flac");

        Assert.False(result.Succeeded);
        Assert.Contains("no such file", result.ErrorOutput);
    }

    [Fact]
    public async Task ConvertAsync_PassesBitrateFlag()
    {
        var runner = new FakeRunner();
        var converter = new FfmpegConverter("ffmpeg", runner);

        await converter.ConvertAsync(@"C:\music\song.flac");

        Assert.Contains("-b:a 320k", runner.LastArguments);
    }

    [Fact]
    public async Task ConvertAsync_OutputIsMp3()
    {
        var runner = new FakeRunner();
        var converter = new FfmpegConverter("ffmpeg", runner);

        await converter.ConvertAsync(@"C:\music\song.flac");

        Assert.Contains(".mp3", runner.LastArguments);
    }

    [Fact]
    public async Task ConvertAsync_OverwriteFlagIsSet()
    {
        var runner = new FakeRunner();
        var converter = new FfmpegConverter("ffmpeg", runner);

        await converter.ConvertAsync(@"C:\music\song.flac");

        Assert.StartsWith("-y ", runner.LastArguments);
    }

    [Fact]
    public async Task ConvertAsync_UsesConfiguredFfmpegPath()
    {
        var runner = new FakeRunner();
        var converter = new FfmpegConverter(@"C:\tools\ffmpeg.exe", runner);

        await converter.ConvertAsync(@"C:\music\song.flac");

        Assert.Equal(@"C:\tools\ffmpeg.exe", runner.LastExecutable);
    }

    [Fact]
    public async Task ConvertAsync_CancellationRequested_ReturnsFailure()
    {
        var runner = new FakeRunner();
        var converter = new FfmpegConverter("ffmpeg", runner);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await converter.ConvertAsync(@"C:\music\song.flac", cts.Token);

        Assert.False(result.Succeeded);
    }
}
