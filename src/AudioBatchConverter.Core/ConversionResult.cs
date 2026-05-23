namespace AudioBatchConverter.Core;

public record ConversionResult(bool Succeeded, string? ErrorOutput)
{
    public static ConversionResult Success() => new(true, null);
    public static ConversionResult Failure(string error) => new(false, error);
}
