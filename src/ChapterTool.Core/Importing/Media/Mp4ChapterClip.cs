namespace ChapterTool.Core.Importing.Media;

/// <summary>
/// Represents an MP4 chapter clip with title and duration.
/// </summary>
/// <param name="Title">The Title value.</param>
/// <param name="Duration">The Duration value.</param>
public sealed record Mp4ChapterClip(string Title, TimeSpan Duration);
