namespace ChapterTool.Infrastructure.Tools;

public static class ExternalToolPathResolver
{
    public static string ExecutableName(string toolId) =>
        OperatingSystem.IsWindows() && !toolId.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? $"{toolId}.exe"
            : toolId;

    public static IEnumerable<string> ExpandConfiguredCandidates(string? configuredPath, string executableName)
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
}
