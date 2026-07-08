namespace ChapterTool.Core.Models;

/// <summary>
/// Represents a single chapter marker with timing, naming, and frame metadata.
/// </summary>
/// <param name="Number">The Number value.</param>
/// <param name="Time">The Time value.</param>
/// <param name="Name">The Name value.</param>
/// <param name="FramesInfo">The FramesInfo value.</param>
/// <param name="End">The End value.</param>
/// <param name="FrameAccuracy">The FrameAccuracy value.</param>
public sealed record Chapter(
    int Number,
    TimeSpan Time,
    string Name,
    string FramesInfo = "",
    TimeSpan? End = null,
    FrameAccuracy FrameAccuracy = FrameAccuracy.Neutral)
{
    /// <summary>
    /// Gets the SeparatorTime value.
    /// </summary>
    public static readonly TimeSpan SeparatorTime = TimeSpan.MinValue;

    /// <summary>
    /// Gets the IsSeparator value.
    /// </summary>
    public bool IsSeparator => Time == SeparatorTime;
}

/// <summary>
/// Identifies whether a chapter time lands exactly on a frame boundary.
/// </summary>
public enum FrameAccuracy
{
    /// <summary>
    /// Frame accuracy was not evaluated.
    /// </summary>
    Neutral,
    /// <summary>
    /// The value is frame accurate.
    /// </summary>
    Accurate,
    /// <summary>
    /// The value does not land exactly on a frame boundary.
    /// </summary>
    Inexact
}
