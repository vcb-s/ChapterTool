using ChapterTool.Core.Importing.Media;
using ChapterTool.Infrastructure.Platform;

namespace ChapterTool.Avalonia.Services;

public sealed class MissingMp4ChapterReader(INativeDependencyService nativeDependencyService) : IMp4ChapterReader
{
    public async ValueTask<Mp4ChapterReadResult> ReadAsync(string path, CancellationToken cancellationToken)
    {
        var dependency = await nativeDependencyService.ResolveAsync("libmp4v2", cancellationToken);
        return Mp4ChapterReadResult.Failed(
            dependency.Found ? "NativeReadFailed" : "NativeLibraryMissing",
            dependency.Found
                ? $"MP4 native reader implementation is not available for {path}."
                : dependency.Message ?? "libmp4v2 was not found.");
    }
}
