namespace ChapterTool.Core.Tests;

public sealed class FixtureResolverTests
{
    [Fact]
    public void ExistingSample_locates_legacy_vtt_fixture()
    {
        var path = FixtureResolver.ExistingSample("Time_Shift_Test", "[VTT_Sample]", "chapter.vtt");

        Assert.EndsWith("chapter.vtt", path, StringComparison.Ordinal);
    }

    [Fact]
    public void ExistingSample_locates_non_ascii_cue_fixture()
    {
        var path = FixtureResolver.ExistingSample(
            "Time_Shift_Test",
            "[cue_Sample]",
            "のんのんびより りぴーと オリジナルサウンドトラック.cue");

        Assert.Contains("のんのんびより", Path.GetFileName(path), StringComparison.Ordinal);
    }
}
