namespace ChapterTool.Core.Models;

/// <summary>
/// Represents a chapter collection and source metadata loaded from or written to a media source.
/// </summary>
/// <param name="Title">The Title value.</param>
/// <param name="SourceName">The SourceName value.</param>
/// <param name="SourceIndex">The SourceIndex value.</param>
/// <param name="SourceType">The SourceType value.</param>
/// <param name="FramesPerSecond">The FramesPerSecond value.</param>
/// <param name="Duration">The Duration value.</param>
/// <param name="Chapters">The Chapters value.</param>
/// <param name="Expression">The Expression value.</param>
/// <param name="Tag">The Tag value.</param>
/// <param name="TagType">The TagType value.</param>
public sealed record ChapterInfo(
    string Title,
    string? SourceName,
    int SourceIndex,
    string SourceType,
    double FramesPerSecond,
    TimeSpan Duration,
    IReadOnlyList<Chapter> Chapters,
    string Expression = "t",
    object? Tag = null,
    string? TagType = null);
