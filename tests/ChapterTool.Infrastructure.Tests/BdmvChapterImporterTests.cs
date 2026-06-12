using ChapterTool.Core.Importing;
using ChapterTool.Core.Services;
using ChapterTool.Core.Transform;
using ChapterTool.Infrastructure.Importing.Bdmv;

namespace ChapterTool.Infrastructure.Tests;

public sealed class BdmvChapterImporterTests
{
    [Fact]
    public async Task ImportAsyncReadsMetadataTitleAndParsesEac3toChapterText()
    {
        var root = CreateBdmvRoot(writeMeta: true);
        var playlistDirectory = Path.Combine(root, "BDMV", "PLAYLIST");
        File.Copy(
            Path.Combine(FixtureResolver.RepositoryRoot, "tests", "ChapterTool.Core.Tests", "Fixtures", "Importing", "Disc", "Mpls", "00001_fch.mpls"),
            Path.Combine(playlistDirectory, "00001.mpls"));
        var runner = new FakeRunner([
            Success("""
                1) 00001.mpls, 00001.m2ts, 01:00:20
                   - Chapters, 9 chapters
                """)
        ]);
        var importer = NewImporter(runner);

        var result = await importer.ImportAsync(new ChapterImportRequest(root), TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        var info = result.Groups.Single().Options.Single().ChapterInfo;
        Assert.Equal("Disc Title", info.Title);
        Assert.Equal("BDMV", info.SourceType);
        Assert.Equal("00001.m2ts", info.SourceName);
        Assert.Equal(9, info.Chapters.Count);
        Assert.Equal([root, "-showall"], runner.Requests[0].Arguments);
        Assert.Single(runner.Requests);
    }

    [Fact]
    public async Task ImportAsyncFailsMissingDependency()
    {
        var importer = new BdmvChapterImporter(new FakeLocator(new ExternalToolLocation(false, null, "MissingDependency", "missing")), new FakeRunner([]), new ChapterTimeFormatter());

        var result = await importer.ImportAsync(new ChapterImportRequest(CreateBdmvRoot()), TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "MissingDependency");
    }

    [Theory]
    [InlineData("", "DependencyOutputUnrecognized")]
    [InlineData("stderr", "DependencyExecutionFailed")]
    public async Task ImportAsyncDiagnosesBadDependencyOutput(string stderr, string code)
    {
        var resultToReturn = stderr.Length == 0
            ? Success("not a playlist")
            : new ProcessRunResult(0, "", stderr, false, false, "eac3to", [], null);
        var importer = NewImporter(new FakeRunner([resultToReturn]));

        var result = await importer.ImportAsync(new ChapterImportRequest(CreateBdmvRoot()), TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == code);
    }

    private static BdmvChapterImporter NewImporter(FakeRunner runner) =>
        new(new FakeLocator(new ExternalToolLocation(true, "eac3to")), runner, new ChapterTimeFormatter());

    private static ProcessRunResult Success(string stdout) =>
        new(0, stdout, "", false, false, "eac3to", [], null);

    private static string CreateBdmvRoot(bool writeMeta = false)
    {
        var root = Path.Combine(Path.GetTempPath(), "ChapterTool.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "BDMV", "PLAYLIST"));
        if (writeMeta)
        {
            var meta = Path.Combine(root, "BDMV", "META", "DL");
            Directory.CreateDirectory(meta);
            File.WriteAllText(Path.Combine(meta, "disc.xml"), "<di:name>Disc Title</di:name>");
        }

        return root;
    }

    private sealed class FakeLocator(ExternalToolLocation location) : IExternalToolLocator
    {
        public ValueTask<ExternalToolLocation> LocateAsync(string toolId, CancellationToken cancellationToken) =>
            ValueTask.FromResult(location);
    }

    private sealed class PathToolLocator : IExternalToolLocator
    {
        public ValueTask<ExternalToolLocation> LocateAsync(string toolId, CancellationToken cancellationToken)
        {
            var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var candidate = Path.Combine(directory, OperatingSystem.IsWindows() ? $"{toolId}.exe" : toolId);
                if (File.Exists(candidate))
                {
                    return ValueTask.FromResult(new ExternalToolLocation(true, candidate));
                }
            }

            return ValueTask.FromResult(new ExternalToolLocation(false, null, "MissingDependency", $"{toolId} was not found."));
        }
    }

    private sealed class FakeRunner(IReadOnlyList<ProcessRunResult> results) : IProcessRunner
    {
        private int index;

        public List<ProcessRunRequest> Requests { get; } = [];

        public ValueTask<ProcessRunResult> RunAsync(ProcessRunRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            var result = results[Math.Min(index, results.Count - 1)];
            index++;
            return ValueTask.FromResult(result with
            {
                FileName = request.FileName,
                Arguments = request.Arguments,
                WorkingDirectory = request.WorkingDirectory
            });
        }
    }
}
