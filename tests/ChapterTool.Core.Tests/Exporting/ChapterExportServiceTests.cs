using ChapterTool.Core.Exporting;
using ChapterTool.Core.Models;
using ChapterTool.Core.Transform;

namespace ChapterTool.Core.Tests.Exporting;

public sealed class ChapterExportServiceTests
{
    private readonly ChapterExportService service = new(new ChapterTimeFormatter(), new ExpressionService());

    [Fact]
    public void Txt_export_writes_ogm_pairs()
    {
        var result = service.Export(Sample(), new ChapterExportOptions(ChapterExportFormat.Txt));

        Assert.Contains("CHAPTER01=00:00:00.000", result.Content, StringComparison.Ordinal);
        Assert.Contains("CHAPTER01NAME=Intro", result.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void Xml_export_uses_und_language_fallback()
    {
        var result = service.Export(Sample(), new ChapterExportOptions(ChapterExportFormat.Xml));

        Assert.Contains("<ChapterLanguage>und</ChapterLanguage>", result.Content, StringComparison.Ordinal);
        Assert.Contains("<ChapterTimeStart>00:00:10.000000</ChapterTimeStart>", result.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void Qpf_export_trims_markers_and_appends_I()
    {
        var result = service.Export(Sample(), new ChapterExportOptions(ChapterExportFormat.Qpf));

        Assert.Equal($"0 I{Environment.NewLine}240 I{Environment.NewLine}480 I", result.Content);
    }

    [Fact]
    public void Tsmuxer_export_has_no_trailing_semicolon()
    {
        var result = service.Export(Sample(), new ChapterExportOptions(ChapterExportFormat.TsMuxerMeta));

        Assert.Equal($"--custom-{Environment.NewLine}chapters=00:00:00.000;00:00:10.000;00:00:20.000", result.Content);
    }

    [Fact]
    public void Cue_export_numbers_tracks_by_output_order()
    {
        var result = service.Export(
            Sample(),
            new ChapterExportOptions(ChapterExportFormat.Cue, SourceFileName: "audio.wav", AutoGenerateNames: true));

        Assert.Contains("FILE \"audio.wav\" WAVE", result.Content, StringComparison.Ordinal);
        Assert.Contains("  TRACK 01 AUDIO", result.Content, StringComparison.Ordinal);
        Assert.Contains("    TITLE \"Chapter 01\"", result.Content, StringComparison.Ordinal);
        Assert.Contains("    INDEX 01 00:10:00", result.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void Json_export_uses_mpls_source_name_and_separator_base_time()
    {
        var info = Sample("MPLS") with
        {
            SourceName = "00001",
            Chapters =
            [
                new Chapter(1, TimeSpan.FromSeconds(5), "A"),
                new Chapter(-1, Chapter.SeparatorTime, ""),
                new Chapter(2, TimeSpan.FromSeconds(7), "B")
            ]
        };

        var result = service.Export(info, new ChapterExportOptions(ChapterExportFormat.Json));

        Assert.Contains("\"sourceName\":\"00001.m2ts\"", result.Content, StringComparison.Ordinal);
        Assert.Contains("\"time\":0", result.Content, StringComparison.Ordinal);
        Assert.Contains("\"time\":2", result.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void Timecodes_export_applies_expression_when_enabled()
    {
        var result = service.Export(
            Sample(),
            new ChapterExportOptions(ChapterExportFormat.TimeCodes, ApplyExpression: true, Expression: "t + 1"));

        Assert.StartsWith("00:00:01.000", result.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void Expression_export_normalizes_negative_times_to_zero()
    {
        var info = Sample() with
        {
            Chapters = [new Chapter(1, TimeSpan.FromSeconds(10), "Middle", "240 K")]
        };
        var result = service.Export(
            info,
            new ChapterExportOptions(ChapterExportFormat.Txt, ApplyExpression: true, Expression: "t - 10000"));

        Assert.Contains("CHAPTER01=00:00:00.000", result.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("-", result.Content, StringComparison.Ordinal);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "InvalidExpressionTime");
    }

    [Fact]
    public void Cue_export_applies_expression_when_enabled()
    {
        var result = service.Export(
            Sample(),
            new ChapterExportOptions(ChapterExportFormat.Cue, ApplyExpression: true, Expression: "t + 1"));

        Assert.Contains("    INDEX 01 00:01:00", result.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void Qpf_export_uses_expression_adjusted_frames_when_enabled()
    {
        var result = service.Export(
            Sample(),
            new ChapterExportOptions(ChapterExportFormat.Qpf, ApplyExpression: true, Expression: "t + 1"));

        Assert.StartsWith("24 I", result.Content, StringComparison.Ordinal);
    }

    private static ChapterInfo Sample(string sourceType = "OGM") =>
        new(
            "Title",
            "source",
            0,
            sourceType,
            24,
            TimeSpan.FromSeconds(30),
            [
                new Chapter(1, TimeSpan.Zero, "Intro", "0 K"),
                new Chapter(2, TimeSpan.FromSeconds(10), "Middle", "240 *"),
                new Chapter(3, TimeSpan.FromSeconds(20), "End", "480 K")
            ]);
}
