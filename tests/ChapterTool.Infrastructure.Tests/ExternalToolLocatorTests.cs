using ChapterTool.Infrastructure.Configuration;
using ChapterTool.Infrastructure.Tools;

namespace ChapterTool.Infrastructure.Tests;

public sealed class ExternalToolLocatorTests
{
    [Fact]
    public async Task LocateAsync_uses_configured_mkvtoolnix_path_before_search_paths()
    {
        var root = CreateTempDirectory();
        var configuredDirectory = Path.Combine(root, "configured");
        Directory.CreateDirectory(configuredDirectory);
        var expectedToolPath = Path.Combine(configuredDirectory, ToolExecutable("mkvextract"));
        await File.WriteAllTextAsync(expectedToolPath, "");

        var searchDirectory = Path.Combine(root, "search");
        Directory.CreateDirectory(searchDirectory);
        await File.WriteAllTextAsync(Path.Combine(searchDirectory, ToolExecutable("mkvextract")), "");

        var settingsStore = new AppSettingsStore(root, [root]);
        await settingsStore.SaveAsync(
            new AppSettings(MkvToolnixPath: configuredDirectory),
            CancellationToken.None);
        var locator = new ExternalToolLocator(settingsStore, [searchDirectory]);

        var location = await locator.LocateAsync("mkvextract", CancellationToken.None);

        Assert.True(location.Found);
        Assert.Equal(expectedToolPath, location.Path);
    }

    [Fact]
    public async Task LocateAsync_returns_missing_dependency_when_tool_is_absent()
    {
        var root = CreateTempDirectory();
        var settingsStore = new AppSettingsStore(root, [root]);
        var locator = new ExternalToolLocator(settingsStore, [root]);

        var location = await locator.LocateAsync("eac3to", CancellationToken.None);

        Assert.False(location.Found);
        Assert.Null(location.Path);
        Assert.Equal("MissingDependency", location.DiagnosticCode);
    }

    private static string ToolExecutable(string name) => OperatingSystem.IsWindows() ? $"{name}.exe" : name;

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "ChapterTool.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
