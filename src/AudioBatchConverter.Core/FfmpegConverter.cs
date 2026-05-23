namespace AudioBatchConverter.Core;

public class FfmpegConverter
{
    private readonly string _ffmpegPath;
    private readonly IProcessRunner _runner;

    /// <summary>
    /// Returns the path to ffmpeg.exe: bundled copy next to the exe first, then PATH.
    /// </summary>
    public static string FindFfmpeg()
    {
        var appDir = Path.GetDirectoryName(Environment.ProcessPath ?? "") ?? "";
        var bundled = Path.Combine(appDir, "ffmpeg.exe");
        return File.Exists(bundled) ? bundled : "ffmpeg";
    }

    public FfmpegConverter(string ffmpegPath = "ffmpeg", IProcessRunner? runner = null)
    {
        _ffmpegPath = ffmpegPath;
        _runner = runner ?? new DefaultProcessRunner();
    }

    public async Task<ConversionResult> ConvertAsync(string sourcePath, CancellationToken ct = default)
    {
        var targetPath = Path.ChangeExtension(sourcePath, ".mp3");
        var args = $"-y -i \"{sourcePath}\" -b:a 320k \"{targetPath}\"";

        try
        {
            var (exitCode, stderr) = await _runner.RunAsync(_ffmpegPath, args, ct);
            return exitCode == 0
                ? ConversionResult.Success()
                : ConversionResult.Failure(stderr.Trim());
        }
        catch (OperationCanceledException)
        {
            return ConversionResult.Failure("Conversion cancelled.");
        }
        catch (Exception ex)
        {
            return ConversionResult.Failure(ex.Message);
        }
    }
}
