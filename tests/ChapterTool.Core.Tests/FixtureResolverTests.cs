namespace ChapterTool.Core.Tests;

public sealed class FixtureResolverTests
{
    [Fact]
    public void Fixture_locates_vtt_fixture()
    {
        var path = FixtureResolver.Fixture("Importing", "Text", "WebVtt", "chapter.vtt");

        Assert.EndsWith("chapter.vtt", path, StringComparison.Ordinal);
    }

    [Fact]
    public void Fixture_locates_non_ascii_cue_fixture()
    {
        var path = FixtureResolver.Fixture(
            "Importing",
            "Cue",
            "のんのんびより りぴーと オリジナルサウンドトラック.cue");

        Assert.Contains("のんのんびより", Path.GetFileName(path), StringComparison.Ordinal);
    }
}
