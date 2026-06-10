using ChapterTool.Avalonia.Services;
using ChapterTool.Avalonia.Composition;
using ChapterTool.Core.Importing.Media;
using ChapterTool.Core.Services;
using ChapterTool.Core.Transform;
using ChapterTool.Infrastructure.Platform;

namespace ChapterTool.Avalonia.Tests;

public sealed class RuntimeChapterLoadServiceTests
{
    [Theory]
    [InlineData(".txt", "CHAPTER01=00:00:00.000\r\nCHAPTER01NAME=Intro\r\n")]
    [InlineData(".vtt", "WEBVTT\r\n\r\n00:00:00.000 --> 00:00:01.000\r\nIntro\r\n")]
    [InlineData(".cue", "FILE \"audio.flac\" WAVE\r\n  TRACK 01 AUDIO\r\n    TITLE \"Intro\"\r\n    INDEX 01 00:00:00\r\n")]
    public async Task RuntimeRoutesTextCueAndWebVttSources(string extension, string content)
    {
        var path = Path.Combine(Path.GetTempPath(), "ChapterTool.Tests", Guid.NewGuid().ToString("N") + extension);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, content);
        try
        {
            var result = await CreateService().LoadAsync(path, CancellationToken.None);

            Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => $"{diagnostic.Code}: {diagnostic.Message}")));
            Assert.Single(result.Groups.Single().Options.Single().ChapterInfo.Chapters);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task RuntimeRoutesXmlSource()
    {
        var path = Path.Combine(Path.GetTempPath(), "ChapterTool.Tests", Guid.NewGuid().ToString("N") + ".xml");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path,
            """
            <?xml version="1.0"?>
            <Chapters>
              <EditionEntry>
                <ChapterAtom>
                  <ChapterTimeStart>00:00:00.000000000</ChapterTimeStart>
                  <ChapterDisplay><ChapterString>Intro</ChapterString></ChapterDisplay>
                </ChapterAtom>
              </EditionEntry>
            </Chapters>
            """);
        try
        {
            var result = await CreateService().LoadAsync(path, CancellationToken.None);

            Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => $"{diagnostic.Code}: {diagnostic.Message}")));
            Assert.Single(result.Groups.Single().Options.Single().ChapterInfo.Chapters);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData(".mp4")]
    [InlineData(".m4a")]
    [InlineData(".m4v")]
    public async Task RuntimeRoutesMp4FamilyThroughInjectedReader(string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), "ChapterTool.Tests", Guid.NewGuid().ToString("N") + extension);
        await File.WriteAllBytesAsync(path, [0]);
        try
        {
            var result = await CreateService(Mp4ChapterReadResult.Succeeded(
                new Mp4ChapterClip("Intro", TimeSpan.FromSeconds(1)),
                new Mp4ChapterClip("Main", TimeSpan.FromSeconds(2))))
                .LoadAsync(path, CancellationToken.None);

            Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => $"{diagnostic.Code}: {diagnostic.Message}")));
            Assert.Equal([TimeSpan.Zero, TimeSpan.FromSeconds(1)], result.Groups.Single().Options.Single().ChapterInfo.Chapters.Select(static chapter => chapter.Time));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task RuntimeSurfacesMp4ReaderFailureDiagnostics()
    {
        var path = Path.Combine(Path.GetTempPath(), "ChapterTool.Tests", Guid.NewGuid().ToString("N") + ".mp4");
        await File.WriteAllBytesAsync(path, [0]);
        try
        {
            var result = await CreateService(Mp4ChapterReadResult.Failed("Mp4ReadFailed", "reader failed"))
                .LoadAsync(path, CancellationToken.None);

            Assert.False(result.Success);
            Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "Mp4ReadFailed");
            Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == "UnsupportedSource");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData(".mkv")]
    [InlineData(".mka")]
    public async Task RuntimeRoutesMatroskaFamilyToMkvextractWithChapterFixture(string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), "ChapterTool.Tests", Guid.NewGuid().ToString("N") + extension);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.Copy(MatroskaFixture(), path);
        try
        {
            var result = await CreateService().LoadAsync(path, CancellationToken.None);

            if (result.Success)
            {
                var chapters = result.Groups.Single().Options.Single().ChapterInfo.Chapters;
                Assert.True(chapters.Count >= 2);
                Assert.Equal(["序章", "Chapter 02"], chapters.Take(2).Select(static chapter => chapter.Name));
                Assert.Equal([TimeSpan.Zero, TimeSpan.FromSeconds(1)], chapters.Take(2).Select(static chapter => chapter.Time));
            }
            else
            {
                Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "MatroskaMissingDependency");
            }

            Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == "UnsupportedSource");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task RuntimeRoutesBdmvDirectoryToBdmvImporter()
    {
        var root = Path.Combine(Path.GetTempPath(), "ChapterTool.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "BDMV", "PLAYLIST"));
        try
        {
            var result = await CreateService().LoadAsync(root, CancellationToken.None);

            Assert.False(result.Success);
            Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "MissingDependency");
            Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == "UnsupportedSource");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RuntimeLoadsExistingIfoFixture()
    {
        var root = RepositoryRoot();
        var path = Path.Combine(root, "Time_Shift_Test", "[ifo_Sample]", "VTS_05_0.IFO");

        var result = await CreateService().LoadAsync(path, CancellationToken.None);

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => $"{diagnostic.Code}: {diagnostic.Message}")));
        var info = result.Groups.Single().Options.Single().ChapterInfo;
        Assert.Equal("DVD", info.SourceType);
        Assert.Equal(7, info.Chapters.Count);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Time_Shift.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test output directory.");
    }

    private static string MatroskaFixture() =>
        Path.Combine(
            RepositoryRoot(),
            "tests",
            "ChapterTool.Infrastructure.Tests",
            "Fixtures",
            "Importing",
            "Matroska",
            "chaptered-small.mkv");

    private static IChapterLoadService CreateService() =>
        new AppCompositionRoot(settingsDirectory: Path.Combine(Path.GetTempPath(), "ChapterTool.Tests", Guid.NewGuid().ToString("N")))
            .CreateChapterLoadService();

    private static IChapterLoadService CreateService(Mp4ChapterReadResult mp4ReadResult) =>
        new RuntimeChapterLoadService(new RuntimeChapterImporterRegistry(
            new ChapterTimeFormatter(),
            new MissingExternalToolLocator(),
            new MissingProcessRunner(),
            new FakeMp4ChapterReader(mp4ReadResult)));

    private sealed class FakeMp4ChapterReader(Mp4ChapterReadResult result) : IMp4ChapterReader
    {
        public ValueTask<Mp4ChapterReadResult> ReadAsync(string path, CancellationToken cancellationToken) =>
            ValueTask.FromResult(result);
    }

    private sealed class MissingExternalToolLocator : IExternalToolLocator
    {
        public ValueTask<ExternalToolLocation> LocateAsync(string toolName, CancellationToken cancellationToken) =>
            ValueTask.FromResult(new ExternalToolLocation(false, null, "MissingDependency", toolName));
    }

    private sealed class MissingProcessRunner : IProcessRunner
    {
        public ValueTask<ProcessRunResult> RunAsync(ProcessRunRequest request, CancellationToken cancellationToken) =>
            ValueTask.FromResult(new ProcessRunResult(-1, string.Empty, string.Empty, TimedOut: false, Cancelled: false, request.FileName, request.Arguments, request.WorkingDirectory));
    }
}
