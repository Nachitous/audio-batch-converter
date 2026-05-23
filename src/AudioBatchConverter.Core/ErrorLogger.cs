namespace AudioBatchConverter.Core;

public static class ErrorLogger
{
    public static void Log(string sourcePath, string message)
    {
        var dir = Path.GetDirectoryName(sourcePath) ?? Directory.GetCurrentDirectory();
        var logPath = Path.Combine(dir, "audio-batch-converter-errors.txt");
        var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {Path.GetFileName(sourcePath)}: {message}";
        File.AppendAllText(logPath, entry + Environment.NewLine);
    }

    public static string GetLogPath(string sourcePath)
    {
        var dir = Path.GetDirectoryName(sourcePath) ?? Directory.GetCurrentDirectory();
        return Path.Combine(dir, "audio-batch-converter-errors.txt");
    }
}
