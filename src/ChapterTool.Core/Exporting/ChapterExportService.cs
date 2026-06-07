using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml.Linq;
using ChapterTool.Core.Diagnostics;
using ChapterTool.Core.Models;
using ChapterTool.Core.Transform;

namespace ChapterTool.Core.Exporting;

public sealed class ChapterExportService(
    IChapterTimeFormatter timeFormatter,
    IExpressionService expressionService)
{
    public ChapterExportResult Export(ChapterInfo info, ChapterExportOptions options)
    {
        return options.Format switch
        {
            ChapterExportFormat.Txt => Text(info, options),
            ChapterExportFormat.Xml => Xml(info, options),
            ChapterExportFormat.Qpf => Lines(".qpf", info.Chapters.Where(NotSeparator).Select(static c => c.FramesInfo.TrimEnd('K', '*') + "I")),
            ChapterExportFormat.TimeCodes => Lines(".TimeCodes.txt", info.Chapters.Where(NotSeparator).Select(c => FormatTime(c, info, options))),
            ChapterExportFormat.TsMuxerMeta => TsMuxer(info, options),
            ChapterExportFormat.Cue => Cue(info, options),
            ChapterExportFormat.Json => Json(info, options),
            _ => Failure("UnsupportedExportFormat", "Unsupported export format.")
        };
    }

    private ChapterExportResult Text(ChapterInfo info, ChapterExportOptions options)
    {
        var builder = new StringBuilder();
        foreach (var (chapter, index) in info.Chapters.Where(NotSeparator).Select((chapter, index) => (chapter, index)))
        {
            var number = chapter.Number <= 0 ? index + 1 : chapter.Number;
            builder.AppendLine(CultureInfo.InvariantCulture, $"CHAPTER{number:D2}={FormatTime(chapter, info, options)}");
            builder.AppendLine(CultureInfo.InvariantCulture, $"CHAPTER{number:D2}NAME={DisplayName(chapter, index, options)}");
        }

        return Success(builder.ToString(), ".txt");
    }

    private ChapterExportResult Xml(ChapterInfo info, ChapterExportOptions options)
    {
        var language = string.IsNullOrWhiteSpace(options.XmlLanguage) ? "und" : options.XmlLanguage;
        var atoms = info.Chapters.Where(NotSeparator).Select((chapter, index) =>
            new XElement(
                "ChapterAtom",
                new XElement("ChapterDisplay", new XElement("ChapterString", DisplayName(chapter, index, options)), new XElement("ChapterLanguage", language)),
                new XElement("ChapterUID", index + 1),
                new XElement("ChapterTimeStart", FormatTime(chapter, info, options) + "000"),
                new XElement("ChapterFlagHidden", "0"),
                new XElement("ChapterFlagEnabled", "1")));
        var document = new XDocument(
            new XElement(
                "Chapters",
                new XElement(
                    "EditionEntry",
                    new XElement("EditionFlagHidden", "0"),
                    new XElement("EditionFlagDefault", "0"),
                    new XElement("EditionUID", "1"),
                    atoms)));
        return Success(document.ToString(SaveOptions.DisableFormatting), ".xml");
    }

    private ChapterExportResult TsMuxer(ChapterInfo info, ChapterExportOptions options)
    {
        var chapters = info.Chapters.Where(NotSeparator).Select(chapter => FormatTime(chapter, info, options)).ToArray();
        if (chapters.Length == 0)
        {
            return Failure("NoChapters", "No chapters are available for tsMuxeR meta export.");
        }

        return Success($"--custom-{Environment.NewLine}chapters={string.Join(';', chapters)}", ".TsMuxeR_Meta.txt");
    }

    private ChapterExportResult Cue(ChapterInfo info, ChapterExportOptions options)
    {
        var builder = new StringBuilder();
        builder.AppendLine("REM Generate By ChapterTool");
        builder.AppendLine(CultureInfo.InvariantCulture, $"TITLE \"{Escape(info.Title)}\"");
        builder.AppendLine(CultureInfo.InvariantCulture, $"FILE \"{Escape(options.SourceFileName ?? info.SourceName ?? string.Empty)}\" WAVE");
        var track = 0;
        foreach (var (chapter, index) in info.Chapters.Where(NotSeparator).Select((chapter, index) => (chapter, index)))
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"  TRACK {++track:D2} AUDIO");
            builder.AppendLine(CultureInfo.InvariantCulture, $"    TITLE \"{Escape(DisplayName(chapter, index, options))}\"");
            builder.AppendLine(CultureInfo.InvariantCulture, $"    INDEX 01 {timeFormatter.FormatCue(chapter.Time)}");
        }

        return Success(builder.ToString(), ".cue");
    }

    private ChapterExportResult Json(ChapterInfo info, ChapterExportOptions options)
    {
        var entries = new List<JsonChapter>();
        var baseTime = TimeSpan.Zero;
        Chapter? previous = null;
        var autoIndex = 0;
        foreach (var chapter in info.Chapters)
        {
            if (chapter.IsSeparator)
            {
                if (previous is not null)
                {
                    baseTime = previous.Time;
                    autoIndex = 0;
                    entries.Add(new JsonChapter(DisplayName(previous, autoIndex++, options), 0));
                }

                continue;
            }

            entries.Add(new JsonChapter(DisplayName(chapter, autoIndex++, options), (chapter.Time - baseTime).TotalSeconds));
            previous = chapter;
        }

        var payload = new JsonPayload(info.SourceType == "MPLS" ? $"{info.SourceName}.m2ts" : null, entries);
        return Success(JsonSerializer.Serialize(payload, JsonOptions), ".json");
    }

    private ChapterExportResult Lines(string extension, IEnumerable<string> lines) =>
        Success(string.Join(Environment.NewLine, lines), extension);

    private static bool NotSeparator(Chapter chapter) => !chapter.IsSeparator;

    private static string Escape(string value) => value.Replace("\"", "\\\"", StringComparison.Ordinal);

    private string FormatTime(Chapter chapter, ChapterInfo info, ChapterExportOptions options)
    {
        var time = chapter.Time;
        if (options.ApplyExpression)
        {
            var evaluated = expressionService.EvaluateInfix(options.Expression, (decimal)time.TotalSeconds, (decimal)info.FramesPerSecond);
            time = TimeSpan.FromSeconds((double)evaluated.Value);
        }

        return timeFormatter.Format(time);
    }

    private static string DisplayName(Chapter chapter, int index, ChapterExportOptions options) =>
        options.AutoGenerateNames ? $"Chapter {index + 1:D2}" : chapter.Name;

    private static ChapterExportResult Success(string content, string extension) =>
        new(true, content, extension, Array.Empty<ChapterDiagnostic>());

    private static ChapterExportResult Failure(string code, string message) =>
        new(false, string.Empty, string.Empty, [new ChapterDiagnostic(DiagnosticSeverity.Error, code, message)]);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private sealed record JsonPayload(string? SourceName, IReadOnlyList<JsonChapter> Chapter);

    private sealed record JsonChapter(string Name, double Time);
}
