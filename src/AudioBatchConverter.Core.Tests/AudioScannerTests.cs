using AudioBatchConverter.Core;
using Xunit;

namespace AudioBatchConverter.Core.Tests;

public class AudioScannerTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public AudioScannerTests() => Directory.CreateDirectory(_tempDir);
    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    private string Touch(string relativePath)
    {
        var full = Path.Combine(_tempDir, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, "");
        return full;
    }

    [Fact]
    public void Scan_SingleFile_MatchingExtension_ReturnsThatFile()
    {
        var file = Touch("song.flac");
        var results = AudioScanner.Scan(file).ToList();
        Assert.Single(results);
        Assert.Equal(file, results[0]);
    }

    [Fact]
    public void Scan_SingleFile_NonAudioExtension_ReturnsEmpty()
    {
        var file = Touch("readme.txt");
        Assert.Empty(AudioScanner.Scan(file));
    }

    [Fact]
    public void Scan_Directory_RecursivelyFindsAudioFiles()
    {
        Touch("a.flac");
        Touch("sub/b.wav");
        Touch("c.txt");

        var results = AudioScanner.Scan(_tempDir).ToList();
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void Scan_CustomExtensions_FiltersCorrectly()
    {
        Touch("a.flac");
        Touch("b.wav");

        var results = AudioScanner.Scan(_tempDir, [".flac"]).ToList();
        Assert.Single(results);
        Assert.EndsWith(".flac", results[0]);
    }

    [Fact]
    public void Scan_NonExistentPath_ReturnsEmpty()
    {
        var results = AudioScanner.Scan(@"C:\this\path\does\not\exist\12345").ToList();
        Assert.Empty(results);
    }

    [Fact]
    public void Scan_ExtensionMatchIsCaseInsensitive()
    {
        Touch("track.FLAC");
        var results = AudioScanner.Scan(_tempDir).ToList();
        Assert.Single(results);
    }

    [Theory]
    [InlineData(".flac")]
    [InlineData(".wav")]
    [InlineData(".ogg")]
    [InlineData(".aac")]
    [InlineData(".m4a")]
    [InlineData(".wma")]
    [InlineData(".opus")]
    public void Scan_DefaultExtensions_IncludesCommonFormats(string ext)
    {
        Touch($"audio{ext}");
        Assert.Single(AudioScanner.Scan(_tempDir));
    }
}
