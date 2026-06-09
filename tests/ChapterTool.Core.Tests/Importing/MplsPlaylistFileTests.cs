using ChapterTool.Core.Importing.Disc;

namespace ChapterTool.Core.Tests.Importing;

public sealed class MplsPlaylistFileTests
{
    [Fact]
    public void ReadMapsSinglePlayItemSampleToWikiAlignedFields()
    {
        using var stream = File.OpenRead(FixtureResolver.Fixture("Importing", "Disc", "Mpls", "00011_eva.mpls"));

        var file = MplsPlaylistFile.Read(stream);

        Assert.Equal("MPLS", file.TypeIndicator);
        Assert.Equal("0200", file.VersionNumber);
        Assert.True(file.PlayListStartAddress > 0);
        Assert.True(file.PlayListMarkStartAddress > file.PlayListStartAddress);
        Assert.True(file.AppInfoPlayList.Length > 0);
        Assert.Equal(8, file.AppInfoPlayList.UOMaskTable.FlagField.Length);
        Assert.Equal(0U, file.ExtensionDataStartAddress);
        Assert.Null(file.ExtensionData);
        Assert.Equal(1, file.PlayList.NumberOfPlayItems);
        Assert.Empty(file.PlayList.SubPaths);

        var playItem = file.PlayList.PlayItems.Single();
        Assert.Equal("00002", playItem.ClipName.ClipInformationFileName);
        Assert.Equal("M2TS", playItem.ClipName.ClipCodecIdentifier);
        Assert.False(playItem.IsMultiAngle);
        Assert.Equal(188460000U, playItem.INTime);
        Assert.Equal(474480000U, playItem.OUTTime);
        Assert.Equal(8, playItem.UOMaskTable.FlagField.Length);
        Assert.Equal(1, playItem.STNTable.NumberOfPrimaryVideoStreamEntries);
        Assert.Empty(playItem.STNTable.SubPathStreamEntries);

        var primaryVideo = playItem.STNTable.PrimaryVideoStreamEntries.First();
        Assert.Equal(0x01, primaryVideo.StreamEntry.StreamType);
        Assert.True(primaryVideo.StreamEntry.RefToStreamPID > 0);
        Assert.Equal(0x1B, primaryVideo.StreamAttributes.StreamCodingType);
        Assert.Equal((byte)2, primaryVideo.StreamAttributes.FrameRate.GetValueOrDefault());

        var firstMark = file.PlayListMark.Marks.First();
        Assert.Equal(0x01, firstMark.MarkType);
        Assert.Equal(0, firstMark.RefToPlayItemID);
        Assert.Equal(188460000U, firstMark.MarkTimeStamp);
        Assert.True(firstMark.EntryESPID > 0);
        Assert.Equal(0U, firstMark.Duration);
    }

    [Fact]
    public void ReadMapsMultiAngleSampleToWikiAlignedFields()
    {
        using var stream = File.OpenRead(FixtureResolver.Fixture("Importing", "Disc", "Mpls", "00002_tanji.mpls"));

        var file = MplsPlaylistFile.Read(stream);

        Assert.Equal(9, file.PlayList.NumberOfPlayItems);
        var multiAngle = file.PlayList.PlayItems[1];
        Assert.True(multiAngle.IsMultiAngle);
        Assert.Equal("00006&00007", multiAngle.FullName);
        Assert.NotNull(multiAngle.MultiAngle);
        Assert.Equal(2, multiAngle.MultiAngle.NumberOfAngles);
        Assert.Single(multiAngle.MultiAngle.Angles);
        Assert.Equal("00007", multiAngle.MultiAngle.Angles.Single().ClipName.ClipInformationFileName);
        Assert.Equal("M2TS", multiAngle.MultiAngle.Angles.Single().ClipName.ClipCodecIdentifier);
        Assert.Equal(24000d / 1001d, MplsFrameRate(multiAngle));

        var marksByPlayItem = file.PlayListMark.Marks
            .Where(static mark => mark.MarkType == 0x01)
            .GroupBy(static mark => mark.RefToPlayItemID)
            .ToDictionary(static group => group.Key, static group => group.Select(mark => mark.MarkTimeStamp).ToArray());
        Assert.Equal([189000000U], marksByPlayItem[0]);
        Assert.False(marksByPlayItem.ContainsKey(1));
        Assert.Equal([195654375U, 216264339U], marksByPlayItem[2]);
    }

    private static double MplsFrameRate(MplsPlayItem playItem)
    {
        var frameRateCode = playItem.STNTable.PrimaryVideoStreamEntries.First().StreamAttributes.FrameRate;
        return frameRateCode switch
        {
            1 => 24000d / 1001d,
            2 => 24,
            3 => 25,
            4 => 30000d / 1001d,
            6 => 50,
            7 => 60000d / 1001d,
            _ => 0
        };
    }
}
