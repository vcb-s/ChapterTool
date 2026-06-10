using ChapterTool.Core.Importing;
using ChapterTool.Core.Services;
using ChapterTool.Core.Transform;
using ChapterTool.Infrastructure.Configuration;
using ChapterTool.Infrastructure.Importing.Matroska;
using ChapterTool.Infrastructure.Processes;
using ChapterTool.Infrastructure.Tools;
using Xunit.Sdk;

namespace ChapterTool.Infrastructure.Tests.Importing;

public sealed class MatroskaIntegrationTests : IAsyncLifetime
{
    private ExternalToolLocation? mkvextractLocation;
    private string? skipReason;

    public Task InitializeAsync()
    {
        var root = Path.Combine(Path.GetTempPath(), "ChapterTool.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var pathDirs = (Environment.GetEnvironmentVariable("PATH") ?? "")
            .Split(Path.PathSeparator)
            .Where(static directory => !string.IsNullOrWhiteSpace(directory))
            .ToArray();

        var locator = new ExternalToolLocator(new AppSettingsStore(root, [root]), pathDirs);
        mkvextractLocation = locator.LocateAsync("mkvextract", CancellationToken.None).AsTask().Result;

        if (!mkvextractLocation.Found)
        {
            skipReason = "mkvextract not found. Install MKVToolNix to run this integration test.";
        }

        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Importer_reads_chapters_from_real_mkv_file()
    {
        RequireMkvToolNix();

        var importer = new MatroskaChapterImporter(
            new SingleToolLocator(mkvextractLocation!),
            new ProcessRunner(),
            new ChapterTimeFormatter());

        var fixturePath = FixtureResolver.Fixture("Importing", "Matroska", "chaptered-small.mkv");
        var result = await importer.ImportAsync(new ChapterImportRequest(fixturePath), CancellationToken.None);

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => $"{diagnostic.Code}: {diagnostic.Message}")));
        var chapters = result.Groups.Single().Options.Single().ChapterInfo.Chapters;
        Assert.Equal(2, chapters.Count);
        Assert.Equal("序章", chapters[0].Name);
        Assert.Equal(TimeSpan.Zero, chapters[0].Time);
        Assert.Equal("Chapter 02", chapters[1].Name);
        Assert.Equal(TimeSpan.FromSeconds(1), chapters[1].Time);
    }

    [Fact]
    public async Task Importer_returns_error_for_nonexistent_mkv_file()
    {
        RequireMkvToolNix();

        var importer = new MatroskaChapterImporter(
            new SingleToolLocator(mkvextractLocation!),
            new ProcessRunner(),
            new ChapterTimeFormatter());

        var result = await importer.ImportAsync(
            new ChapterImportRequest(Path.Combine(Path.GetTempPath(), "nonexistent.mkv")),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "MatroskaProcessFailed");
    }

    private void RequireMkvToolNix()
    {
        if (skipReason is not null)
        {
            throw new XunitException(skipReason);
        }
    }

    private sealed class SingleToolLocator(ExternalToolLocation location) : IExternalToolLocator
    {
        public ValueTask<ExternalToolLocation> LocateAsync(string toolId, CancellationToken cancellationToken)
        {
            Assert.Equal("mkvextract", toolId);
            return ValueTask.FromResult(location);
        }
    }
}
