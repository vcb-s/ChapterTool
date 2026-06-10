using ChapterTool.Core.Services;
using ChapterTool.Infrastructure.Configuration;

namespace ChapterTool.Infrastructure.Tools;

public sealed class ExternalToolLocator(
    ISettingsStore<AppSettings> settingsStore,
    IReadOnlyList<string>? searchDirectories = null,
    IMkvToolNixInstallProbe? mkvToolNixInstallProbe = null)
    : IExternalToolLocator
{
    private readonly IMkvToolNixInstallProbe mkvToolNixInstallProbe = mkvToolNixInstallProbe ?? MkvToolNixInstallProbe.CreateDefault();

    public async ValueTask<ExternalToolLocation> LocateAsync(string toolId, CancellationToken cancellationToken)
    {
        var settings = await settingsStore.LoadAsync(cancellationToken);
        var configuredPath = GetConfiguredPath(toolId, settings);
        var executableName = ExecutableName(toolId);

        foreach (var candidate in ExpandCandidates(configuredPath, executableName))
        {
            if (File.Exists(candidate))
            {
                return new ExternalToolLocation(true, candidate);
            }
        }

        foreach (var directory in searchDirectories ?? [])
        {
            var candidate = Path.Combine(directory, executableName);
            if (File.Exists(candidate))
            {
                return new ExternalToolLocation(true, candidate);
            }
        }

        if (toolId.Equals("mkvextract", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var candidate in mkvToolNixInstallProbe.FindMkvExtractCandidates(executableName))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (File.Exists(candidate))
                {
                    return new ExternalToolLocation(true, candidate);
                }
            }
        }

        return new ExternalToolLocation(
            false,
            null,
            "MissingDependency",
            $"External tool '{toolId}' was not found.");
    }

    private static string? GetConfiguredPath(string toolId, AppSettings settings)
    {
        return toolId.Equals("mkvextract", StringComparison.OrdinalIgnoreCase)
            ? settings.MkvToolnixPath
            : toolId.Equals("eac3to", StringComparison.OrdinalIgnoreCase)
                ? settings.Eac3toPath
                : null;
    }

    private static IEnumerable<string> ExpandCandidates(string? configuredPath, string executableName)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            yield break;
        }

        if (Directory.Exists(configuredPath))
        {
            yield return Path.Combine(configuredPath, executableName);
            yield break;
        }

        yield return configuredPath;
    }

    private static string ExecutableName(string toolId) =>
        OperatingSystem.IsWindows() && !toolId.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? $"{toolId}.exe"
            : toolId;
}
