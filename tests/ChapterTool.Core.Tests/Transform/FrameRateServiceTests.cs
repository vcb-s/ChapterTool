using ChapterTool.Core.Models;
using ChapterTool.Core.Transform;

namespace ChapterTool.Core.Tests.Transform;

public sealed class FrameRateServiceTests
{
    private readonly FrameRateService _service = new();

    [Fact]
    public void Options_expose_legacy_mpls_frame_rate_codes_without_combo_box_indexes()
    {
        var options = _service.Options;

        Assert.Collection(
            options,
            option => AssertFrameRate(option, "Auto", "Auto", 0m, false, 0),
            option => AssertFrameRate(option, "Fps23976", "24000 / 1001", 24000m / 1001m, true, 1),
            option => AssertFrameRate(option, "Fps24", "24000 / 1000", 24m, true, 2),
            option => AssertFrameRate(option, "Fps25", "25000 / 1000", 25m, true, 3),
            option => AssertFrameRate(option, "Fps2997", "30000 / 1001", 30000m / 1001m, true, 4),
            option => AssertFrameRate(option, "Reserved", "RESER / VED", 0m, false, 5),
            option => AssertFrameRate(option, "Fps50", "50000 / 1000", 50m, true, 6),
            option => AssertFrameRate(option, "Fps5994", "60000 / 1001", 60000m / 1001m, true, 7));
    }

    [Fact]
    public void FindByValue_matches_valid_frame_rate_with_legacy_tolerance()
    {
        var actual = _service.FindByValue(23.976023976m);

        Assert.Equal("Fps23976", actual.Code);
    }

    [Fact]
    public void Detect_skips_auto_and_reserved_options()
    {
        var chapters = new[]
        {
            new Chapter(1, TimeSpan.Zero, "Chapter 1"),
            new Chapter(2, TimeSpan.FromSeconds(7d / 25d), "Chapter 2"),
            new Chapter(3, TimeSpan.FromSeconds(8d / 25d), "Chapter 3"),
        };
        var info = NewInfo(0m, chapters);

        var actual = _service.Detect(info, tolerance: 0.01m);

        Assert.Equal("Fps25", actual.Code);
    }

    [Fact]
    public void Detect_returns_first_highest_scoring_valid_option_on_tie()
    {
        var chapters = new[]
        {
            new Chapter(1, TimeSpan.Zero, "Chapter 1"),
            new Chapter(2, TimeSpan.FromSeconds(1), "Chapter 2"),
        };
        var info = NewInfo(0m, chapters);

        var actual = _service.Detect(info, tolerance: 0.15m);

        Assert.Equal("Fps23976", actual.Code);
    }

    [Fact]
    public void UpdateFrames_uses_detected_option_when_rounding_with_auto_option()
    {
        var chapters = new[]
        {
            new Chapter(1, TimeSpan.Zero, "Chapter 1"),
            new Chapter(2, TimeSpan.FromSeconds(7d / 25d), "Chapter 2"),
        };
        var info = NewInfo(0m, chapters);

        var actual = _service.UpdateFrames(info, _service.Options[0], round: true, tolerance: 0.01m);

        Assert.Equal("Fps25", actual.SelectedOption.Code);
        Assert.Equal(25m, actual.FramesPerSecond);
        Assert.Equal(new[] { "0 K", "7 K" }, actual.Chapters.Select(chapter => chapter.FramesInfo));
    }

    [Fact]
    public void UpdateFrames_marks_rounded_frames_with_k_when_difference_is_less_than_tolerance()
    {
        var info = NewInfo(0m, new[] { new Chapter(1, TimeSpan.FromSeconds(1.0004), "Chapter 1") });

        var actual = _service.UpdateFrames(info, _service.Options[3], round: true, tolerance: 0.15m);

        Assert.Equal("25 K", actual.Chapters.Single().FramesInfo);
    }

    [Fact]
    public void UpdateFrames_marks_rounded_frames_with_star_when_difference_is_not_less_than_tolerance()
    {
        var info = NewInfo(0m, new[] { new Chapter(1, TimeSpan.FromSeconds(1.004), "Chapter 1") });

        var actual = _service.UpdateFrames(info, _service.Options[3], round: true, tolerance: 0.01m);

        Assert.Equal("25 *", actual.Chapters.Single().FramesInfo);
    }

    [Fact]
    public void UpdateFrames_uses_away_from_zero_midpoint_rounding()
    {
        var info = NewInfo(0m, new[] { new Chapter(1, TimeSpan.FromSeconds(0.5), "Chapter 1") });

        var actual = _service.UpdateFrames(info, _service.Options[3], round: true, tolerance: 0.01m);

        Assert.Equal("13 *", actual.Chapters.Single().FramesInfo);
    }

    [Fact]
    public void UpdateFrames_displays_raw_decimal_without_marker_when_rounding_is_disabled()
    {
        var info = NewInfo(0m, new[] { new Chapter(1, TimeSpan.FromSeconds(0.5), "Chapter 1") });

        var actual = _service.UpdateFrames(info, _service.Options[2], round: false, tolerance: 0.15m);

        Assert.Equal("12.0", actual.Chapters.Single().FramesInfo);
    }

    private static ChapterInfo NewInfo(decimal fps, IReadOnlyList<Chapter> chapters)
    {
        return new ChapterInfo(
            Title: "Title",
            SourceName: null,
            SourceIndex: 0,
            SourceType: "OGM",
            FramesPerSecond: (double)fps,
            Duration: TimeSpan.Zero,
            Chapters: chapters);
    }

    private static void AssertFrameRate(
        FrameRateOption option,
        string code,
        string displayName,
        decimal value,
        bool isValid,
        int legacyMplsCode)
    {
        Assert.Equal(code, option.Code);
        Assert.Equal(displayName, option.DisplayName);
        Assert.Equal(value, option.Value);
        Assert.Equal(isValid, option.IsValid);
        Assert.Equal(legacyMplsCode, option.LegacyMplsCode);
    }
}
