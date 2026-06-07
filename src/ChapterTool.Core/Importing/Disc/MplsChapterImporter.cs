using ChapterTool.Core.Diagnostics;
using ChapterTool.Core.Models;

namespace ChapterTool.Core.Importing.Disc;

public sealed class MplsChapterImporter : IChapterImporter
{
    private static readonly double[] FrameRates =
    [
        0,
        24000d / 1001d,
        24,
        25,
        30000d / 1001d,
        0,
        50,
        60000d / 1001d
    ];

    public string Id => "mpls";

    public IReadOnlySet<string> SupportedExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".mpls"
    };

    public async ValueTask<ChapterImportResult> ImportAsync(ChapterImportRequest request, CancellationToken cancellationToken)
    {
        await using var stream = request.Content ?? File.OpenRead(request.Path);
        try
        {
            var parsed = Parse(stream);
            var options = parsed.PlayItems.Select((playItem, index) => ToOption(request.Path, playItem, parsed.Marks, index)).ToArray();
            return new ChapterImportResult(true, [new ChapterInfoGroup(request.Path, options, 0)], Array.Empty<ChapterDiagnostic>());
        }
        catch (Exception exception) when (exception is InvalidDataException or EndOfStreamException or IOException)
        {
            return ChapterImportResult.Failed(new ChapterDiagnostic(DiagnosticSeverity.Error, "InvalidMpls", exception.Message));
        }
    }

    public static TimeSpan PtsToTime(uint pts)
    {
        var total = pts / 45000M;
        var seconds = Math.Floor(total);
        var milliseconds = Math.Round((total - seconds) * 1000M, MidpointRounding.AwayFromZero);
        return new TimeSpan(0, 0, 0, (int)seconds, (int)milliseconds);
    }

    private static ChapterSourceOption ToOption(string path, PlayItem playItem, IReadOnlyList<Mark> marks, int playItemIndex)
    {
        var matchingMarks = marks
            .Where(mark => mark.MarkType == 0x01 && mark.RefToPlayItemId == playItemIndex)
            .ToArray();
        var offset = matchingMarks.Length == 0 ? playItem.InTime : matchingMarks[0].Timestamp;
        if (playItem.InTime < offset)
        {
            offset = playItem.InTime;
        }

        var chapters = matchingMarks.Length == 0
            ? [new Chapter(1, TimeSpan.Zero, "Chapter 1")]
            : matchingMarks.Select((mark, index) => new Chapter(index + 1, PtsToTime(mark.Timestamp - offset), $"Chapter {index + 1:D2}")).ToArray();
        var info = new ChapterInfo(
            string.Empty,
            playItem.FullName,
            playItemIndex,
            "MPLS",
            playItem.FrameRateCode < FrameRates.Length ? FrameRates[playItem.FrameRateCode] : 0,
            PtsToTime(playItem.OutTime - playItem.InTime),
            chapters,
            Tag: path,
            TagType: "MPLS");
        return new ChapterSourceOption($"clip-{playItemIndex}", $"{playItem.FullName}__{chapters.Length}", info, CanCombine: true, MediaReferences: playItem.MediaReferences);
    }

    private static ParsedMpls Parse(Stream stream)
    {
        var header = stream.ReadAscii(4);
        if (header != "MPLS")
        {
            throw new InvalidDataException("Invalid MPLS header.");
        }

        var version = stream.ReadAscii(4);
        if (version is not ("0100" or "0200" or "0300"))
        {
            throw new InvalidDataException($"Unsupported MPLS version: {version}.");
        }

        var playListAddress = stream.ReadUInt32BigEndian();
        var markAddress = stream.ReadUInt32BigEndian();
        stream.ReadUInt32BigEndian();

        stream.Position = playListAddress;
        var playItems = ReadPlayList(stream);

        stream.Position = markAddress;
        var marks = ReadMarks(stream);
        return new ParsedMpls(playItems, marks);
    }

    private static IReadOnlyList<PlayItem> ReadPlayList(Stream stream)
    {
        var length = stream.ReadUInt32BigEndian();
        var position = stream.Position;
        stream.SkipBytes(2);
        var playItemCount = stream.ReadUInt16BigEndian();
        var subPathCount = stream.ReadUInt16BigEndian();
        var items = new List<PlayItem>(playItemCount);
        for (var i = 0; i < playItemCount; i++)
        {
            items.Add(ReadPlayItem(stream));
        }

        for (var i = 0; i < subPathCount; i++)
        {
            var subPathLength = stream.ReadUInt32BigEndian();
            stream.SkipBytes(subPathLength);
        }

        stream.SkipBytes(length - (stream.Position - position));
        return items;
    }

    private static PlayItem ReadPlayItem(Stream stream)
    {
        var length = stream.ReadUInt16BigEndian();
        var position = stream.Position;
        var clipName = stream.ReadAscii(5);
        stream.ReadAscii(4);
        var flags = stream.ReadUInt16BigEndian();
        stream.SkipBytes(1);
        var inTime = stream.ReadUInt32BigEndian();
        var outTime = stream.ReadUInt32BigEndian();
        stream.SkipBytes(8);
        stream.SkipBytes(4);
        var isMultiAngle = ((flags >> 4) & 1) == 1;
        var fullName = clipName;
        if (isMultiAngle)
        {
            var angleCount = stream.ReadByte();
            stream.SkipBytes(1);
            for (var i = 0; i < angleCount - 1; i++)
            {
                fullName += "&" + stream.ReadAscii(5);
                stream.SkipBytes(5);
            }
        }

        var frameRateCode = ReadPrimaryVideoFrameRateCode(stream);
        stream.SkipBytes(length - (stream.Position - position));
        var refs = fullName
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(clip => new SourceMediaReference($"{clip}.m2ts", Path.Combine("..", "STREAM", $"{clip}.m2ts")))
            .ToArray();
        return new PlayItem(fullName, inTime, outTime, frameRateCode, refs);
    }

    private static int ReadPrimaryVideoFrameRateCode(Stream stream)
    {
        var length = stream.ReadUInt16BigEndian();
        var position = stream.Position;
        stream.SkipBytes(2);
        var primaryVideoCount = stream.ReadByte();
        var streamCounts = new byte[6];
        stream.ReadExactly(streamCounts);
        stream.SkipBytes(5);
        if (primaryVideoCount == 0)
        {
            throw new InvalidDataException("MPLS play item has no primary video stream.");
        }

        var streamEntryLength = stream.ReadByte();
        var streamEntryPosition = stream.Position;
        var streamType = stream.ReadByte();
        if (streamType is 0x02 or 0x04)
        {
            stream.SkipBytes(2);
        }

        stream.SkipBytes(2);
        stream.SkipBytes(streamEntryLength - (stream.Position - streamEntryPosition));
        var attributesLength = stream.ReadByte();
        var attributesPosition = stream.Position;
        stream.SkipBytes(1);
        var videoInfo = stream.ReadByte();
        stream.SkipBytes(attributesLength - (stream.Position - attributesPosition));
        stream.SkipBytes(length - (stream.Position - position));
        return videoInfo & 0x0f;
    }

    private static IReadOnlyList<Mark> ReadMarks(Stream stream)
    {
        var length = stream.ReadUInt32BigEndian();
        var position = stream.Position;
        var markCount = stream.ReadUInt16BigEndian();
        var marks = new List<Mark>(markCount);
        for (var i = 0; i < markCount; i++)
        {
            stream.SkipBytes(1);
            var type = stream.ReadByte();
            var refId = stream.ReadUInt16BigEndian();
            var timestamp = stream.ReadUInt32BigEndian();
            stream.SkipBytes(6);
            marks.Add(new Mark(type, refId, timestamp));
        }

        stream.SkipBytes(length - (stream.Position - position));
        return marks;
    }

    private sealed record ParsedMpls(IReadOnlyList<PlayItem> PlayItems, IReadOnlyList<Mark> Marks);

    private sealed record PlayItem(string FullName, uint InTime, uint OutTime, int FrameRateCode, IReadOnlyList<SourceMediaReference> MediaReferences);

    private sealed record Mark(int MarkType, int RefToPlayItemId, uint Timestamp);
}
