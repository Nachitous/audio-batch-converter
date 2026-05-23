namespace AudioBatchConverter.Core;

public class JobStatusChangedEventArgs(ConversionJob job) : EventArgs
{
    public ConversionJob Job { get; } = job;
}

public class ConversionEngine
{
    private readonly FfmpegConverter _converter;
    private readonly List<ConversionJob> _jobs = [];

    public IReadOnlyList<ConversionJob> Jobs => _jobs;

    public event EventHandler<JobStatusChangedEventArgs>? JobStatusChanged;
    public event EventHandler<int>? ProgressChanged;

    public ConversionEngine(FfmpegConverter? converter = null)
    {
        _converter = converter ?? new FfmpegConverter();
    }

    public void LoadPaths(IEnumerable<string> paths)
    {
        _jobs.Clear();
        foreach (var path in paths)
            _jobs.AddRange(AudioScanner.Scan(path).Select(f => new ConversionJob(f)));
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        for (int i = 0; i < _jobs.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var job = _jobs[i];
            job.Status = JobStatus.InProgress;
            RaiseJobChanged(job);

            var result = await _converter.ConvertAsync(job.SourcePath, ct);

            if (result.Succeeded)
            {
                job.Status = JobStatus.Done;
                TryDeleteOriginal(job.SourcePath);
            }
            else
            {
                job.Status = JobStatus.Error;
                job.ErrorMessage = result.ErrorOutput;
                ErrorLogger.Log(job.SourcePath, result.ErrorOutput ?? "Unknown error");
            }

            RaiseJobChanged(job);
            ProgressChanged?.Invoke(this, i + 1);
        }
    }

    private static void TryDeleteOriginal(string path)
    {
        try { File.Delete(path); }
        catch { /* leave original if delete fails */ }
    }

    private void RaiseJobChanged(ConversionJob job) =>
        JobStatusChanged?.Invoke(this, new JobStatusChangedEventArgs(job));
}
