using ChapterTool.Avalonia.Services;
using ChapterTool.Core.Exporting;
using ChapterTool.Core.Models;
using ChapterTool.Core.Transform;

namespace ChapterTool.Avalonia.Tests;

public sealed class RuntimeChapterSaveServiceTests
{
    [Fact]
    public async Task RuntimeSaveWritesCueBesideRequestedDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "ChapterTool.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var info = new ChapterInfo(
            "audio",
            "audio.flac",
            0,
            "CUE",
            75,
            TimeSpan.FromMinutes(1),
            [new Chapter(1, TimeSpan.Zero, "Intro")]);
        var service = new RuntimeChapterSaveService(new ChapterExportService(new ChapterTimeFormatter(), new ExpressionService()));

        try
        {
            var result = await service.SaveAsync(info, new ChapterExportOptions(ChapterExportFormat.Cue), directory, CancellationToken.None);

            Assert.True(result.Success);
            var path = Path.Combine(directory, "audio.cue");
            Assert.True(File.Exists(path));
            Assert.Contains("TRACK 01 AUDIO", await File.ReadAllTextAsync(path));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
