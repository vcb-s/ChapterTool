using ChapterTool.Core.Importing;

namespace ChapterTool.Avalonia.Services;

public interface IChapterLoadService
{
    ValueTask<ChapterImportResult> LoadAsync(string path, CancellationToken cancellationToken);
}
