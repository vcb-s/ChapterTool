namespace ChapterTool.Core.Importing.Media;

/// <summary>
/// Represents raw chapter metadata read from a media container.
/// </summary>
/// <param name="Id">The Id value.</param>
/// <param name="TimeBase">The TimeBase value.</param>
/// <param name="Start">The Start value.</param>
/// <param name="End">The End value.</param>
/// <param name="StartTime">The StartTime value.</param>
/// <param name="EndTime">The EndTime value.</param>
/// <param name="Tags">The Tags value.</param>
/// <param name="SourceOrder">The SourceOrder value.</param>
public sealed record MediaChapterEntry(
    int? Id,
    string? TimeBase,
    long? Start,
    long? End,
    string? StartTime,
    string? EndTime,
    IReadOnlyDictionary<string, string> Tags,
    int SourceOrder);
