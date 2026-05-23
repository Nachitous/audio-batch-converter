using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace AudioBatchConverter.Worker;

public enum JobStatus { Queued, Converting, Done, Error }

public sealed record TrackedJob(
    string Path,
    JobStatus Status,
    bool KeepOriginals,
    double? ElapsedSeconds = null,
    string? Error = null,
    long SourceBytes = 0,
    long ConvertedBytes = 0);

public sealed class ConversionTracker
{
    private readonly ConcurrentDictionary<string, TrackedJob> _jobs =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Channel<string> _channel =
        Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleReader = true });

    private readonly string _persistencePath;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public ConversionTracker(IOptions<WorkerSettings> settings)
    {
        _persistencePath = settings.Value.PersistencePath;
        LoadFromDisk();
    }

    // ── write side ───────────────────────────────────────────────────────────

    public bool Enqueue(string path, bool keepOriginals)
    {
        var job = new TrackedJob(path, JobStatus.Queued, keepOriginals);
        if (!_jobs.TryAdd(path, job))
        {
            return false;
        }

        _channel.Writer.TryWrite(path);
        SaveToDisk();
        return true;
    }

    public void SetConverting(string path, long sourceBytes = 0) =>
        UpdateAndSave(path, j => j with { Status = JobStatus.Converting, SourceBytes = sourceBytes });

    public void SetDone(string path, double elapsed, long convertedBytes = 0) =>
        UpdateAndSave(path, j => j with
        {
            Status = JobStatus.Done,
            ElapsedSeconds = elapsed,
            ConvertedBytes = convertedBytes,
        });

    public void SetError(string path, string error) =>
        UpdateAndSave(path, j => j with { Status = JobStatus.Error, Error = error });

    public int ClearDone()
    {
        var keys = _jobs
            .Where(kvp => kvp.Value.Status is JobStatus.Done or JobStatus.Error)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keys)
        {
            _jobs.TryRemove(key, out _);
        }

        SaveToDisk();
        return keys.Count;
    }

    private void UpdateAndSave(string path, Func<TrackedJob, TrackedJob> transform)
    {
        if (_jobs.TryGetValue(path, out var job))
        {
            _jobs[path] = transform(job);
        }

        SaveToDisk();
    }

    // ── read side ─────────────────────────────────────────────────────────────

    public ChannelReader<string> Reader => _channel.Reader;

    public TrackedJob? GetJob(string path) =>
        _jobs.TryGetValue(path, out var j) ? j : null;

    // ── API ───────────────────────────────────────────────────────────────────

    public IReadOnlyList<TrackedJob> GetAll() =>
        _jobs.Values.ToList();

    public (int queued, int converting, int done, int errors, long savedBytes) GetStats()
    {
        var all = _jobs.Values.ToList();
        var saved = all
            .Where(j => j.Status == JobStatus.Done && j.SourceBytes > 0)
            .Sum(j => j.SourceBytes - j.ConvertedBytes);

        return (
            all.Count(j => j.Status == JobStatus.Queued),
            all.Count(j => j.Status == JobStatus.Converting),
            all.Count(j => j.Status == JobStatus.Done),
            all.Count(j => j.Status == JobStatus.Error),
            saved);
    }

    // ── persistence ───────────────────────────────────────────────────────────

    private void LoadFromDisk()
    {
        if (!File.Exists(_persistencePath))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(_persistencePath);
            var dtos = JsonSerializer.Deserialize<List<PersistedJob>>(json, _jsonOpts);
            if (dtos is null)
            {
                return;
            }

            foreach (var dto in dtos)
            {
                // Jobs that were mid-flight when the process stopped cannot be resumed —
                // mark them as errors so the user knows to re-queue if needed.
                var status = dto.Status is JobStatus.Done or JobStatus.Error
                    ? dto.Status
                    : JobStatus.Error;

                var error = status == JobStatus.Error && dto.Status is not JobStatus.Error
                    ? "Interrupted by restart"
                    : dto.Error;

                _jobs[dto.Path] = new TrackedJob(
                    dto.Path, status, dto.KeepOriginals,
                    dto.ElapsedSeconds, error,
                    dto.SourceBytes, dto.ConvertedBytes);
            }
        }
        catch
        {
            // Corrupt or unreadable persistence file — start fresh.
        }
    }

    private void SaveToDisk()
    {
        try
        {
            var dtos = _jobs.Values.Select(j => new PersistedJob
            {
                Path           = j.Path,
                Status         = j.Status,
                KeepOriginals  = j.KeepOriginals,
                ElapsedSeconds = j.ElapsedSeconds,
                Error          = j.Error,
                SourceBytes    = j.SourceBytes,
                ConvertedBytes = j.ConvertedBytes,
            }).ToList();

            File.WriteAllText(_persistencePath, JsonSerializer.Serialize(dtos, _jsonOpts));
        }
        catch
        {
            // Persistence failure is non-fatal — in-memory state is authoritative.
        }
    }

    private sealed class PersistedJob
    {
        public string Path { get; set; } = "";
        public JobStatus Status { get; set; }
        public bool KeepOriginals { get; set; }
        public double? ElapsedSeconds { get; set; }
        public string? Error { get; set; }
        public long SourceBytes { get; set; }
        public long ConvertedBytes { get; set; }
    }
}
