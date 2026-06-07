using ChapterTool.Core.Importing;
using ChapterTool.Core.Importing.Disc;
using ChapterTool.Core.Importing.Media;
using ChapterTool.Core.Models;
using ChapterTool.Core.Transform;

namespace ChapterTool.Core.Tests.Importing;

public sealed class DiscImporterTests
{
    [Fact]
    public async Task MplsImporterReadsSinglePlayItemSample()
    {
        var importer = new MplsChapterImporter();
        var result = await importer.ImportAsync(
            new ChapterImportRequest(FixtureResolver.ExistingSample("Time_Shift_Test", "[mpls_Sample]", "00011_eva.mpls")),
            CancellationToken.None);

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => $"{diagnostic.Code}: {diagnostic.Message}")));
        var option = result.Groups.Single().Options.Single();
        var info = option.ChapterInfo;
        Assert.Equal("MPLS", info.SourceType);
        Assert.Equal("00002", info.SourceName);
        Assert.Equal(24, info.FramesPerSecond);
        Assert.Equal(46, info.Chapters.Count);
        Assert.Equal(TimeSpan.Zero, info.Chapters[0].Time);
        Assert.Equal(TimeSpan.FromMilliseconds(14417), info.Chapters[1].Time);
        Assert.Contains(option.MediaReferences ?? [], reference => reference.RelativePath == Path.Combine("..", "STREAM", "00002.m2ts"));
    }

    [Fact]
    public async Task MplsImporterReadsMultiAngleSample()
    {
        var importer = new MplsChapterImporter();
        var result = await importer.ImportAsync(
            new ChapterImportRequest(FixtureResolver.ExistingSample("Time_Shift_Test", "[mpls_Sample]", "00002_tanji.mpls")),
            CancellationToken.None);

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => $"{diagnostic.Code}: {diagnostic.Message}")));
        Assert.Equal(9, result.Groups.Single().Options.Count);
        Assert.Equal("00006&00007", result.Groups.Single().Options[1].ChapterInfo.SourceName);
        Assert.True(result.Groups.Single().Options[1].ChapterInfo.Chapters.Count >= 1);
    }

    [Fact]
    public async Task MplsImporterRejectsInvalidHeader()
    {
        var importer = new MplsChapterImporter();
        using var stream = new MemoryStream("BAD!"u8.ToArray());

        var result = await importer.ImportAsync(new ChapterImportRequest("bad.mpls", stream), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "InvalidMpls");
    }

    [Fact]
    public async Task IfoImporterReadsExistingSample()
    {
        var importer = new IfoChapterImporter();
        var result = await importer.ImportAsync(
            new ChapterImportRequest(FixtureResolver.ExistingSample("Time_Shift_Test", "[ifo_Sample]", "VTS_05_0.IFO")),
            CancellationToken.None);

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => $"{diagnostic.Code}: {diagnostic.Message}")));
        var option = result.Groups.Single().Options.Single();
        var info = option.ChapterInfo;
        Assert.Equal("DVD", info.SourceType);
        Assert.Equal("VTS_05_1", info.SourceName);
        Assert.Equal(7, info.Chapters.Count);
        Assert.Equal("Chapter 07", info.Chapters[6].Name);
        Assert.Equal("01:49:12.679", new ChapterTimeFormatter().Format(info.Chapters[6].Time));
        Assert.Contains(option.MediaReferences ?? [], reference => reference.RelativePath == "VTS_05_1.VOB");
    }

    [Fact]
    public void IfoPlaybackTimeConvertsNtscAndPal()
    {
        var ntsc = IfoChapterImporter.ConvertDvdPlaybackTime(0x00, 0x00, 0x01, 0xC0 | 0x15, out var isNtsc);
        var pal = IfoChapterImporter.ConvertDvdPlaybackTime(0x00, 0x00, 0x01, 0x40 | 0x10, out var isPalNtsc);

        Assert.True(isNtsc);
        Assert.False(isPalNtsc);
        Assert.True(ntsc > TimeSpan.FromSeconds(1.5));
        Assert.Equal(TimeSpan.FromSeconds(1.4), pal);
    }

    [Fact]
    public async Task IfoImporterRejectsInvalidStructure()
    {
        var importer = new IfoChapterImporter();
        using var stream = new MemoryStream("bad"u8.ToArray());
        var path = Path.GetTempFileName();
        await File.WriteAllBytesAsync(path, stream.ToArray());

        try
        {
            var result = await importer.ImportAsync(new ChapterImportRequest(path), CancellationToken.None);
            Assert.False(result.Success);
            Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "InvalidIfo");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task XplImporterReadsSyntheticTitle()
    {
        var importer = new XplChapterImporter();
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(
            """
            <Playlist xmlns="http://www.dvdforum.org/2005/HDDVDVideo/Playlist">
              <TitleSet timeBase="60fps" tickBase="24fps">
                <Title id="title-id" displayName="Main" titleDuration="00:10:00:00" tickBaseDivisor="1">
                  <PrimaryAudioVideoClip src="ADV_OBJ/main.evo" />
                  <ChapterList>
                    <Chapter id="c1" displayName="Start" titleTimeBegin="00:00:00:00" />
                    <Chapter id="c2" displayName="Middle" titleTimeBegin="00:01:00:12" />
                  </ChapterList>
                </Title>
              </TitleSet>
            </Playlist>
            """));

        var result = await importer.ImportAsync(new ChapterImportRequest("movie.xpl", stream), CancellationToken.None);

        Assert.True(result.Success);
        var info = result.Groups.Single().Options.Single().ChapterInfo;
        Assert.Equal("HD-DVD", info.SourceType);
        Assert.Equal("Main", info.Title);
        Assert.Equal("ADV_OBJ/main.evo", info.SourceName);
        Assert.Equal(TimeSpan.FromSeconds(60.5), info.Chapters[1].Time);
        Assert.Contains(result.Groups.Single().Options.Single().MediaReferences ?? [], reference => reference.RelativePath == Path.Combine("..", "HVDVD_TS", "main.evo"));
    }

    [Theory]
    [InlineData("<Playlist />")]
    [InlineData("<Playlist xmlns=\"http://www.dvdforum.org/2005/HDDVDVideo/Playlist\"><TitleSet><Title><ChapterList><Chapter /></ChapterList></Title></TitleSet></Playlist>")]
    [InlineData("<Playlist xmlns=\"http://www.dvdforum.org/2005/HDDVDVideo/Playlist\"><TitleSet timeBase=\"bad\"><Title titleDuration=\"00:00:01:00\"><ChapterList><Chapter titleTimeBegin=\"bad\" /></ChapterList></Title></TitleSet></Playlist>")]
    public async Task XplImporterDiagnosesMalformedXml(string xml)
    {
        var importer = new XplChapterImporter();
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));

        var result = await importer.ImportAsync(new ChapterImportRequest("bad.xpl", stream), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code is "XplParseFailed" or "XplNoChapters");
    }

    [Fact]
    public async Task Mp4ImporterConvertsDurationsToCumulativeStarts()
    {
        var importer = new Mp4ChapterImporter(new FakeMp4Reader(Mp4ChapterReadResult.Succeeded(
            new Mp4ChapterClip("Chapter 01", TimeSpan.FromSeconds(10)),
            new Mp4ChapterClip("Chapter 02", TimeSpan.FromSeconds(10)),
            new Mp4ChapterClip("Chapter 03", TimeSpan.FromSeconds(10)),
            new Mp4ChapterClip("Chapter 04", TimeSpan.FromSeconds(10)))));

        var result = await importer.ImportAsync(new ChapterImportRequest(FixtureResolver.ExistingSample("Time_Shift_Test", "[Video_Sample]", "Chapter.mp4")), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal([TimeSpan.Zero, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30)], result.Groups.Single().Options.Single().ChapterInfo.Chapters.Select(chapter => chapter.Time));
        Assert.Contains(result.Groups.Single().Options.Single().MediaReferences ?? [], reference => reference.AbsolutePath == FixtureResolver.ExistingSample("Time_Shift_Test", "[Video_Sample]", "Chapter.mp4"));
    }

    [Theory]
    [InlineData("NativeLibraryMissing")]
    [InlineData("NativeReadFailed")]
    public async Task Mp4ImporterReturnsReaderDiagnostics(string code)
    {
        var importer = new Mp4ChapterImporter(new FakeMp4Reader(Mp4ChapterReadResult.Failed(code, "reader failed")));

        var result = await importer.ImportAsync(new ChapterImportRequest("movie.mp4"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == code);
    }

    private sealed class FakeMp4Reader(Mp4ChapterReadResult result) : IMp4ChapterReader
    {
        public ValueTask<Mp4ChapterReadResult> ReadAsync(string path, CancellationToken cancellationToken) =>
            ValueTask.FromResult(result);
    }
}
