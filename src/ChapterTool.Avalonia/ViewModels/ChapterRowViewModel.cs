using ChapterTool.Core.Models;
using ChapterTool.Core.Transform;

namespace ChapterTool.Avalonia.ViewModels;

public sealed class ChapterRowViewModel
{
    public ChapterRowViewModel(Chapter chapter, IChapterTimeFormatter formatter)
    {
        Chapter = chapter;
        TimeText = chapter.IsSeparator ? string.Empty : formatter.Format(chapter.Time);
        Name = chapter.Name;
        FramesInfo = chapter.FramesInfo;
    }

    public Chapter Chapter { get; }

    public int Number => Chapter.Number;

    public string TimeText { get; set; }

    public string Name { get; set; }

    public string FramesInfo { get; set; }
}
