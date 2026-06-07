namespace ChapterTool.Core.Services;

public interface IClipboardService
{
    ValueTask<string?> GetTextAsync(CancellationToken cancellationToken);

    ValueTask SetTextAsync(string text, CancellationToken cancellationToken);
}
