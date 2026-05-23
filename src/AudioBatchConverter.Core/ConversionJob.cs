namespace AudioBatchConverter.Core;

public enum JobStatus { Pending, InProgress, Done, Error }

public class ConversionJob
{
    public string SourcePath { get; }
    public string TargetPath { get; }
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public string? ErrorMessage { get; set; }
    public long SourceBytes { get; set; }
    public long ConvertedBytes { get; set; }

    public ConversionJob(string sourcePath)
    {
        SourcePath = sourcePath;
        TargetPath = Path.ChangeExtension(sourcePath, ".mp3");
    }
}
