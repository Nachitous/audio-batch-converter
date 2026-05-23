namespace AudioBatchConverter.Worker;

public sealed class WorkerSettings
{
    public string FfmpegPath { get; set; } = "ffmpeg";
    public string Extensions { get; set; } = ".flac,.wav,.ogg,.m4a,.aac,.wma,.opus";

    // Root directory the filesystem browser is allowed to navigate within.
    // Set to /volume1 on the NAS; map that volume in docker-compose.
    public string BrowserRoot { get; set; } = "/";

    // Path where job state is persisted between restarts. Relative to CWD (/app in Docker).
    public string PersistencePath { get; set; } = "jobs.json";

    public HashSet<string> GetExtensionSet() =>
        new(
            Extensions
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(e => e.StartsWith('.') ? e.ToLowerInvariant() : $".{e.ToLowerInvariant()}"),
            StringComparer.OrdinalIgnoreCase);
}
