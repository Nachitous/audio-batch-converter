using System.Diagnostics;
using AudioBatchConverter.Core;
using Microsoft.Extensions.Options;

namespace AudioBatchConverter.Worker;

public sealed class ConversionWorker : BackgroundService
{
    private readonly WorkerSettings _settings;
    private readonly ConversionTracker _tracker;
    private readonly ILogger<ConversionWorker> _logger;

    public ConversionWorker(
        IOptions<WorkerSettings> settings,
        ConversionTracker tracker,
        ILogger<ConversionWorker> logger)
    {
        _settings = settings.Value;
        _tracker = tracker;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audio Batch Converter ready — browser root: {Root}", _settings.BrowserRoot);

        var converter = new FfmpegConverter(_settings.FfmpegPath);

        await foreach (var path in _tracker.Reader.ReadAllAsync(stoppingToken))
        {
            await ConvertFileAsync(converter, path, stoppingToken);
        }
    }

    private async Task ConvertFileAsync(FfmpegConverter converter, string path, CancellationToken ct)
    {
        var job = _tracker.GetJob(path);
        if (job is null)
        {
            return;
        }

        if (File.Exists(Path.ChangeExtension(path, ".mp3")))
        {
            _tracker.SetDone(path, 0);
            return;
        }

        _tracker.SetConverting(path);
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Converting  {File}", path);

        try
        {
            var result = await converter.ConvertAsync(path, ct);

            if (result.Succeeded)
            {
                _tracker.SetDone(path, sw.Elapsed.TotalSeconds);

                if (!job.KeepOriginals)
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Could not delete original: {File} — {Error}", path, ex.Message);
                    }
                }

                _logger.LogInformation("Done        {File} ({Elapsed:0.0}s)", path, sw.Elapsed.TotalSeconds);
            }
            else
            {
                _tracker.SetError(path, result.ErrorOutput ?? "Unknown error");
                _logger.LogError("Failed      {File} — {Error}", path, result.ErrorOutput);
            }
        }
        catch (OperationCanceledException)
        {
            _tracker.SetError(path, "Cancelled");
        }
        catch (Exception ex)
        {
            _tracker.SetError(path, ex.Message);
            _logger.LogError(ex, "Error       {File}", path);
        }
    }
}
