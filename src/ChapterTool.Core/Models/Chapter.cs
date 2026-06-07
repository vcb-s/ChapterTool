namespace ChapterTool.Core.Models;

public sealed record Chapter(
    int Number,
    TimeSpan Time,
    string Name,
    string FramesInfo = "",
    TimeSpan? End = null)
{
    public static readonly TimeSpan SeparatorTime = TimeSpan.MinValue;

    public bool IsSeparator => Time == SeparatorTime;
}
