using System.Globalization;
using ChapterTool.Core.Diagnostics;
using ChapterTool.Core.Models;
using ChapterTool.Core.Transform;

namespace ChapterTool.Core.Exporting;

public sealed class ChapterConversionService
{
    public ChapterConversionResult ToCelltimes(ChapterInfo info, decimal framesPerSecond)
    {
        if (framesPerSecond <= 0)
        {
            return Failure("InvalidFrameRate", "Frame rate must be greater than zero.");
        }

        var lines = info.Chapters
            .Where(static chapter => !chapter.IsSeparator)
            .Select(chapter => ChapterRounding
                .RoundToInt64((decimal)chapter.Time.TotalSeconds * framesPerSecond)
                .ToString(CultureInfo.InvariantCulture));

        return Success(string.Join(Environment.NewLine, lines), ".celltimes.txt");
    }

    private static ChapterConversionResult Success(string content, string extension) =>
        new(true, content, extension, []);

    private static ChapterConversionResult Failure(string code, string message) =>
        new(false, string.Empty, string.Empty, [new ChapterDiagnostic(DiagnosticSeverity.Error, code, message)]);
}
