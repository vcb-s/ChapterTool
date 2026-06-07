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
        var runner = new FakeRunner([
            Success("1) 00001.mpls, 00:01:00, 00001.m2ts"),
            Success("""
                CHAPTER01=00:00:00.000
                CHAPTER01NAME=Intro
                CHAPTER02=00:00:10.000
                CHAPTER02NAME=Middle
                """)
        ]);
        var importer = NewImporter(runner);

        var result = await importer.ImportAsync(new ChapterImportRequest(root), CancellationToken.None);

        Assert.True(result.Success);
        var info = result.Groups.Single().Options.Single().ChapterInfo;
        Assert.Equal("Disc Title", info.Title);
        Assert.Equal("BDMV", info.SourceType);
        Assert.Equal("00001.m2ts", info.SourceName);
        Assert.Equal(2, info.Chapters.Count);
        Assert.Equal([root], runner.Requests[0].Arguments);
        Assert.Equal([root, "1)", "chapters.txt"], runner.Requests[1].Arguments);
    }

    [Fact]
    public async Task ImportAsyncFailsMissingDependency()
    {
        var importer = new BdmvChapterImporter(new FakeLocator(new ExternalToolLocation(false, null, "MissingDependency", "missing")), new FakeRunner([]), new ChapterTimeFormatter());

        var result = await importer.ImportAsync(new ChapterImportRequest(CreateBdmvRoot()), CancellationToken.None);

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

        var result = await importer.ImportAsync(new ChapterImportRequest(CreateBdmvRoot()), CancellationToken.None);

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

    private sealed class FakeRunner(IReadOnlyList<ProcessRunResult> results) : IProcessRunner
    {
        private int index;

        public List<ProcessRunRequest> Requests { get; } = [];

        public ValueTask<ProcessRunResult> RunAsync(ProcessRunRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            var result = results[index++];
            return ValueTask.FromResult(result with
            {
                FileName = request.FileName,
                Arguments = request.Arguments,
                WorkingDirectory = request.WorkingDirectory
            });
        }
    }
}
