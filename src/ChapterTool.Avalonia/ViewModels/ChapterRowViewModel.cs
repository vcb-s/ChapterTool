using ChapterTool.Core.Models;
using ChapterTool.Core.Transform;

namespace ChapterTool.Avalonia.ViewModels;

public sealed class ChapterRowViewModel(
    Chapter chapter,
    IChapterTimeFormatter formatter,
    int? number = null,
    string? name = null)
{
    public Chapter Chapter { get; } = chapter;

    public int Number { get; } = number ?? chapter.Number;

    public string TimeText { get; set; } = chapter.IsSeparator ? string.Empty : formatter.Format(chapter.Time);

    public string Name { get; set; } = name ?? chapter.Name;

    public string FramesInfo { get; set; } = chapter.FramesInfo;
}
