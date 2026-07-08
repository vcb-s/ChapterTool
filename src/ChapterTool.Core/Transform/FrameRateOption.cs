namespace ChapterTool.Core.Transform;

/// <summary>
/// Describes a supported frame rate option.
/// </summary>
/// <param name="Code">The Code value.</param>
/// <param name="DisplayName">The DisplayName value.</param>
/// <param name="Value">The Value value.</param>
/// <param name="IsValid">The IsValid value.</param>
/// <param name="LegacyMplsCode">The LegacyMplsCode value.</param>
public sealed record FrameRateOption(
    string Code,
    string DisplayName,
    decimal Value,
    bool IsValid,
    int LegacyMplsCode);
