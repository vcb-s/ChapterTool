using ChapterTool.Core.Importing;
using ChapterTool.Core.Importing.Cue;
using ChapterTool.Core.Importing.Disc;
using ChapterTool.Core.Importing.Media;
using ChapterTool.Core.Importing.Text;
using ChapterTool.Core.Diagnostics;
using ChapterTool.Core.Services;
using ChapterTool.Core.Transform;
using ChapterTool.Infrastructure.Configuration;
using ChapterTool.Infrastructure.Importing.Bdmv;
using ChapterTool.Infrastructure.Importing.Matroska;
using ChapterTool.Infrastructure.Platform;
using ChapterTool.Infrastructure.Processes;
using ChapterTool.Infrastructure.Tools;

namespace ChapterTool.Avalonia.Services;

public sealed class RuntimeChapterLoadService(IChapterTimeFormatter formatter) : IChapterLoadService
{
    public ValueTask<ChapterImportResult> LoadAsync(string path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path) || (!File.Exists(path) && !Directory.Exists(path)))
        {
            return ValueTask.FromResult(ChapterImportResult.Failed(new ChapterDiagnostic(DiagnosticSeverity.Error, "InvalidPath", "The source path does not exist.")));
        }

        var extension = Path.GetExtension(path);
        var toolLocator = CreateToolLocator();
        var processRunner = new ProcessRunner();
        IChapterImporter? importer = Directory.Exists(path) && Directory.Exists(Path.Combine(path, "BDMV", "PLAYLIST"))
            ? new BdmvChapterImporter(toolLocator, processRunner, formatter)
            : extension.ToLowerInvariant() switch
        {
            ".txt" => new OgmChapterImporter(formatter),
            ".xml" => new XmlChapterImporter(formatter),
            ".vtt" => new WebVttChapterImporter(),
            ".cue" => new CueChapterImporter(),
            ".flac" => new FlacCueImporter(),
            ".tak" => new TakCueImporter(),
            ".mpls" => new MplsChapterImporter(),
            ".ifo" => new IfoChapterImporter(),
            ".xpl" => new XplChapterImporter(),
            ".mkv" or ".mka" => new MatroskaChapterImporter(toolLocator, processRunner, formatter),
            ".mp4" or ".m4a" or ".m4v" => new Mp4ChapterImporter(new MissingMp4ChapterReader(CreateNativeDependencyService())),
            _ => null
        };

        return importer is null
            ? ValueTask.FromResult(ChapterImportResult.Failed(new ChapterDiagnostic(DiagnosticSeverity.Error, "UnsupportedSource", $"Unsupported source extension: {extension}.")))
            : importer.ImportAsync(new ChapterImportRequest(path), cancellationToken);
    }

    private static IExternalToolLocator CreateToolLocator()
    {
        var settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ChapterTool");
        return new ExternalToolLocator(
            new AppSettingsStore(settingsDirectory),
            PathSearchDirectories().ToArray());
    }

    private static INativeDependencyService CreateNativeDependencyService() =>
        new FileSystemNativeDependencyService(PathSearchDirectories().Prepend(AppContext.BaseDirectory).ToArray());

    private static IEnumerable<string> PathSearchDirectories()
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var part in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return part;
        }
    }
}
