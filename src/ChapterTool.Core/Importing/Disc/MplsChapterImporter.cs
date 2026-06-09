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
            var parsed = MplsPlaylistFile.Read(stream);
            var options = parsed.PlayList.PlayItems.Select((playItem, index) => ToOption(request.Path, playItem, parsed.PlayListMark.Marks, index)).ToArray();
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

    private static ChapterSourceOption ToOption(string path, MplsPlayItem playItem, IReadOnlyList<MplsMark> marks, int playItemIndex)
    {
        var matchingMarks = marks
            .Where(mark => mark.MarkType == 0x01 && mark.RefToPlayItemID == playItemIndex)
            .ToArray();
        var offset = matchingMarks.Length == 0 ? playItem.INTime : matchingMarks[0].MarkTimeStamp;
        if (playItem.INTime < offset)
        {
            offset = playItem.INTime;
        }

        var chapters = matchingMarks.Length == 0
            ? [new Chapter(1, TimeSpan.Zero, "Chapter 1")]
            : matchingMarks.Select((mark, index) => new Chapter(index + 1, PtsToTime(mark.MarkTimeStamp - offset), $"Chapter {index + 1:D2}")).ToArray();
        var frameRateCode = playItem.STNTable.PrimaryVideoStreamEntries.FirstOrDefault()?.StreamAttributes.FrameRate ?? 0;
        var info = new ChapterInfo(
            string.Empty,
            playItem.FullName,
            playItemIndex,
            "MPLS",
            frameRateCode < FrameRates.Length ? FrameRates[frameRateCode] : 0,
            PtsToTime(playItem.OUTTime - playItem.INTime),
            chapters,
            Tag: path,
            TagType: "MPLS");
        var refs = playItem.FullName
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(clip => new SourceMediaReference($"{clip}.m2ts", Path.Combine("..", "STREAM", $"{clip}.m2ts")))
            .ToArray();
        return new ChapterSourceOption($"clip-{playItemIndex}", $"{playItem.FullName}__{chapters.Length}", info, CanCombine: true, MediaReferences: refs);
    }
}
