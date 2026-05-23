namespace AudioBatchConverter.Core;

public static class AudioScanner
{
    public static readonly IReadOnlyList<string> DefaultExtensions =
        [".flac", ".wav", ".ogg", ".aac", ".m4a", ".wma", ".opus", ".ape", ".alac"];

    public static IEnumerable<string> Scan(string path, IEnumerable<string>? extensions = null)
    {
        var exts = new HashSet<string>(
            (extensions ?? DefaultExtensions).Select(e => e.ToLowerInvariant()),
            StringComparer.OrdinalIgnoreCase);

        if (File.Exists(path))
        {
            if (exts.Contains(Path.GetExtension(path).ToLowerInvariant()))
                yield return path;
            yield break;
        }

        if (!Directory.Exists(path))
            yield break;

        var opts = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
        };

        foreach (var file in Directory.EnumerateFiles(path, "*", opts))
        {
            if (exts.Contains(Path.GetExtension(file).ToLowerInvariant()))
                yield return file;
        }
    }
}
