using System.Collections.Concurrent;
using System.Threading.Channels;

namespace AudioBatchConverter.Worker;

public enum JobStatus { Queued, Converting, Done, Error }

public sealed record TrackedJob(
    string Path,
    JobStatus Status,
    bool KeepOriginals,
    double? ElapsedSeconds = null,
    string? Error = null);

public sealed class ConversionTracker
{
    private readonly ConcurrentDictionary<string, TrackedJob> _jobs =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Channel<string> _channel =
        Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleReader = true });

    // ── write side ───────────────────────────────────────────────────────────

    public bool Enqueue(string path, bool keepOriginals)
    {
        var job = new TrackedJob(path, JobStatus.Queued, keepOriginals);
        if (!_jobs.TryAdd(path, job))
        {
            return false;
        }

        _channel.Writer.TryWrite(path);
        return true;
    }

    public void SetConverting(string path) =>
        Update(path, j => j with { Status = JobStatus.Converting });

    public void SetDone(string path, double elapsed) =>
        Update(path, j => j with { Status = JobStatus.Done, ElapsedSeconds = elapsed });

    public void SetError(string path, string error) =>
        Update(path, j => j with { Status = JobStatus.Error, Error = error });

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

        return keys.Count;
    }

    private void Update(string path, Func<TrackedJob, TrackedJob> transform)
    {
        if (_jobs.TryGetValue(path, out var job))
        {
            _jobs[path] = transform(job);
        }
    }

    // ── read side ─────────────────────────────────────────────────────────────

    public ChannelReader<string> Reader => _channel.Reader;

    public TrackedJob? GetJob(string path) =>
        _jobs.TryGetValue(path, out var j) ? j : null;

    // ── API ───────────────────────────────────────────────────────────────────

    public IReadOnlyList<TrackedJob> GetAll() =>
        _jobs.Values.ToList();

    public (int queued, int converting, int done, int errors) GetStats()
    {
        var all = _jobs.Values.ToList();
        return (
            all.Count(j => j.Status == JobStatus.Queued),
            all.Count(j => j.Status == JobStatus.Converting),
            all.Count(j => j.Status == JobStatus.Done),
            all.Count(j => j.Status == JobStatus.Error));
    }
}
