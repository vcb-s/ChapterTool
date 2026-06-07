using System.Text.RegularExpressions;
using ChapterTool.Core.Diagnostics;
using ChapterTool.Core.Importing;
using ChapterTool.Core.Importing.Text;
using ChapterTool.Core.Models;
using ChapterTool.Core.Services;
using ChapterTool.Core.Transform;

namespace ChapterTool.Infrastructure.Importing.Bdmv;

public sealed partial class BdmvChapterImporter : IChapterImporter
{
    private readonly IExternalToolLocator toolLocator;
    private readonly IProcessRunner processRunner;
    private readonly OgmChapterImporter ogmImporter;

    public BdmvChapterImporter(IExternalToolLocator toolLocator, IProcessRunner processRunner, IChapterTimeFormatter formatter)
    {
        this.toolLocator = toolLocator;
        this.processRunner = processRunner;
        ogmImporter = new OgmChapterImporter(formatter);
    }

    public string Id => "bdmv";

    public IReadOnlySet<string> SupportedExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "BDMV"
    };

    public async ValueTask<ChapterImportResult> ImportAsync(ChapterImportRequest request, CancellationToken cancellationToken)
    {
        var playlistDirectory = Path.Combine(request.Path, "BDMV", "PLAYLIST");
        if (!Directory.Exists(playlistDirectory))
        {
            return ChapterImportResult.Failed(Error("InvalidStructure", "Blu-ray BDMV/PLAYLIST directory was not found."));
        }

        var location = await toolLocator.LocateAsync("eac3to", cancellationToken);
        if (!location.Found || string.IsNullOrWhiteSpace(location.Path))
        {
            return ChapterImportResult.Failed(Error("MissingDependency", location.Message ?? "eac3to was not found."));
        }

        var listResult = await RunAsync(location.Path, request.Path, [request.Path], cancellationToken);
        if (!listResult.Success)
        {
            return listResult;
        }

        var candidates = ParsePlaylistList(listResult.Diagnostics.SingleOrDefault()?.Message ?? string.Empty);
        if (candidates.Count == 0)
        {
            return ChapterImportResult.Failed(Error("DependencyOutputUnrecognized", "eac3to playlist output was not recognized."));
        }

        var options = new List<ChapterSourceOption>();
        foreach (var candidate in candidates)
        {
            var chapterResult = await RunAsync(location.Path, request.Path, [request.Path, $"{candidate.Index})", "chapters.txt"], cancellationToken);
            if (!chapterResult.Success)
            {
                return chapterResult;
            }

            var text = chapterResult.Diagnostics.SingleOrDefault()?.Message ?? string.Empty;
            var parsed = ogmImporter.ImportText(text, request.Path);
            if (!parsed.Success)
            {
                continue;
            }

            var info = parsed.Groups.Single().Options.Single().ChapterInfo with
            {
                Title = ReadDiscTitle(request.Path),
                SourceName = candidate.SourceName,
                SourceIndex = candidate.Index,
                SourceType = "BDMV",
                Duration = candidate.Duration
            };
            options.Add(new ChapterSourceOption($"playlist-{candidate.Index}", $"{candidate.SourceName}__{info.Chapters.Count}", info));
        }

        if (options.Count == 0)
        {
            return ChapterImportResult.Failed(Error("NoChaptersFound", "No BDMV playlists with chapters were parsed."));
        }

        return new ChapterImportResult(true, [new ChapterInfoGroup(request.Path, options, 0)], Array.Empty<ChapterDiagnostic>());
    }

    private async ValueTask<ChapterImportResult> RunAsync(string executable, string workingDirectory, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var result = await processRunner.RunAsync(new ProcessRunRequest(executable, arguments, workingDirectory, TimeSpan.FromSeconds(60)), cancellationToken);
        if (result.Cancelled)
        {
            return ChapterImportResult.Failed(Error("DependencyExecutionCancelled", "eac3to was cancelled."));
        }

        if (result.TimedOut)
        {
            return ChapterImportResult.Failed(Error("DependencyExecutionTimedOut", "eac3to timed out."));
        }

        if (result.ExitCode is not 0 || (string.IsNullOrWhiteSpace(result.StandardOutput) && !string.IsNullOrWhiteSpace(result.StandardError)))
        {
            return ChapterImportResult.Failed(Error("DependencyExecutionFailed", result.StandardError.Length == 0 ? "eac3to failed." : result.StandardError));
        }

        return new ChapterImportResult(true, Array.Empty<ChapterInfoGroup>(), [new ChapterDiagnostic(DiagnosticSeverity.Info, "Stdout", result.StandardOutput)]);
    }

    private static IReadOnlyList<PlaylistCandidate> ParsePlaylistList(string text)
    {
        var candidates = new List<PlaylistCandidate>();
        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var match = PlaylistLineRegex().Match(line);
            if (!match.Success)
            {
                continue;
            }

            var duration = TimeSpan.TryParse(match.Groups["Duration"].Value, out var parsedDuration) ? parsedDuration : TimeSpan.Zero;
            candidates.Add(new PlaylistCandidate(
                int.Parse(match.Groups["Index"].Value),
                match.Groups["Source"].Value,
                duration));
        }

        return candidates;
    }

    private static string ReadDiscTitle(string root)
    {
        var meta = Path.Combine(root, "BDMV", "META", "DL");
        if (!Directory.Exists(meta))
        {
            return string.Empty;
        }

        var file = Directory.EnumerateFiles(meta, "*.xml").FirstOrDefault();
        if (file is null)
        {
            return string.Empty;
        }

        var text = File.ReadAllText(file);
        var match = DiscTitleRegex().Match(text);
        return match.Success ? match.Groups["Title"].Value : string.Empty;
    }

    private static ChapterDiagnostic Error(string code, string message) =>
        new(DiagnosticSeverity.Error, code, message);

    private sealed record PlaylistCandidate(int Index, string SourceName, TimeSpan Duration);

    [GeneratedRegex(@"^\s*(?<Index>\d+)\)\s+.*?\.mpls.*?(?<Duration>\d{1,2}:\d{2}:\d{2}).*?(?<Source>\d+\.m2ts)", RegexOptions.IgnoreCase)]
    private static partial Regex PlaylistLineRegex();

    [GeneratedRegex(@"<di:name>\s*(?<Title>.*?)\s*</di:name>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex DiscTitleRegex();
}
