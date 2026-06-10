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
        var locator = new ExternalToolLocator(settingsStore, [searchDirectory], new FakeMkvToolNixInstallProbe(Path.Combine(root, "probe")));

        var location = await locator.LocateAsync("mkvextract", CancellationToken.None);

        Assert.True(location.Found);
        Assert.Equal(expectedToolPath, location.Path);
    }

    [Fact]
    public async Task LocateAsync_uses_configured_mkvextract_executable_before_search_paths()
    {
        var root = CreateTempDirectory();
        var configuredExecutable = Path.Combine(root, ToolExecutable("custom-mkvextract"));
        await File.WriteAllTextAsync(configuredExecutable, "");

        var searchDirectory = Path.Combine(root, "search");
        Directory.CreateDirectory(searchDirectory);
        await File.WriteAllTextAsync(Path.Combine(searchDirectory, ToolExecutable("mkvextract")), "");

        var settingsStore = new AppSettingsStore(root, [root]);
        await settingsStore.SaveAsync(
            new AppSettings(MkvToolnixPath: configuredExecutable),
            CancellationToken.None);
        var locator = new ExternalToolLocator(settingsStore, [searchDirectory], new FakeMkvToolNixInstallProbe(Path.Combine(root, "probe")));

        var location = await locator.LocateAsync("mkvextract", CancellationToken.None);

        Assert.True(location.Found);
        Assert.Equal(configuredExecutable, location.Path);
    }

    [Fact]
    public async Task LocateAsync_uses_search_path_before_platform_install_discovery()
    {
        var root = CreateTempDirectory();
        var searchDirectory = Path.Combine(root, "search");
        Directory.CreateDirectory(searchDirectory);
        var expectedToolPath = Path.Combine(searchDirectory, ToolExecutable("mkvextract"));
        await File.WriteAllTextAsync(expectedToolPath, "");
        var platformToolPath = Path.Combine(root, "platform", ToolExecutable("mkvextract"));
        Directory.CreateDirectory(Path.GetDirectoryName(platformToolPath)!);
        await File.WriteAllTextAsync(platformToolPath, "");

        var locator = new ExternalToolLocator(
            new AppSettingsStore(root, [root]),
            [searchDirectory],
            new FakeMkvToolNixInstallProbe(platformToolPath));

        var location = await locator.LocateAsync("mkvextract", CancellationToken.None);

        Assert.True(location.Found);
        Assert.Equal(expectedToolPath, location.Path);
    }

    [Fact]
    public async Task LocateAsync_uses_platform_mkvtoolnix_discovery_for_mkvextract()
    {
        var root = CreateTempDirectory();
        var platformToolPath = Path.Combine(root, "platform", ToolExecutable("mkvextract"));
        Directory.CreateDirectory(Path.GetDirectoryName(platformToolPath)!);
        await File.WriteAllTextAsync(platformToolPath, "");

        var locator = new ExternalToolLocator(
            new AppSettingsStore(root, [root]),
            [],
            new FakeMkvToolNixInstallProbe(platformToolPath));

        var location = await locator.LocateAsync("mkvextract", CancellationToken.None);

        Assert.True(location.Found);
        Assert.Equal(platformToolPath, location.Path);
    }

    [Fact]
    public async Task LocateAsync_does_not_use_platform_discovery_for_eac3to()
    {
        var root = CreateTempDirectory();
        var platformToolPath = Path.Combine(root, "platform", ToolExecutable("mkvextract"));
        Directory.CreateDirectory(Path.GetDirectoryName(platformToolPath)!);
        await File.WriteAllTextAsync(platformToolPath, "");

        var locator = new ExternalToolLocator(
            new AppSettingsStore(root, [root]),
            [],
            new FakeMkvToolNixInstallProbe(platformToolPath));

        var location = await locator.LocateAsync("eac3to", CancellationToken.None);

        Assert.False(location.Found);
        Assert.Null(location.Path);
        Assert.Equal("MissingDependency", location.DiagnosticCode);
    }

    [Fact]
    public async Task LocateAsync_returns_missing_dependency_when_tool_is_absent()
    {
        var root = CreateTempDirectory();
        var settingsStore = new AppSettingsStore(root, [root]);
        var locator = new ExternalToolLocator(settingsStore, [root], new FakeMkvToolNixInstallProbe());

        var location = await locator.LocateAsync("eac3to", CancellationToken.None);

        Assert.False(location.Found);
        Assert.Null(location.Path);
        Assert.Equal("MissingDependency", location.DiagnosticCode);
    }

    [Fact]
    public async Task LocateAsync_uses_configured_ffprobe_executable_before_ffmpeg_directory_and_search_paths()
    {
        var root = CreateTempDirectory();
        var configuredExecutable = Path.Combine(root, ToolExecutable("custom-ffprobe"));
        await File.WriteAllTextAsync(configuredExecutable, "");
        var ffmpegDirectory = Path.Combine(root, "ffmpeg");
        Directory.CreateDirectory(ffmpegDirectory);
        await File.WriteAllTextAsync(Path.Combine(ffmpegDirectory, ToolExecutable("ffprobe")), "");
        var searchDirectory = Path.Combine(root, "search");
        Directory.CreateDirectory(searchDirectory);
        await File.WriteAllTextAsync(Path.Combine(searchDirectory, ToolExecutable("ffprobe")), "");

        var settingsStore = new AppSettingsStore(root, [root]);
        await settingsStore.SaveAsync(
            new AppSettings(FfprobePath: configuredExecutable, FfmpegPath: ffmpegDirectory),
            CancellationToken.None);
        var locator = new ExternalToolLocator(settingsStore, [searchDirectory], new FakeMkvToolNixInstallProbe());

        var location = await locator.LocateAsync("ffprobe", CancellationToken.None);

        Assert.True(location.Found);
        Assert.Equal(configuredExecutable, location.Path);
    }

    [Fact]
    public async Task LocateAsync_uses_configured_ffprobe_directory()
    {
        var root = CreateTempDirectory();
        var configuredDirectory = Path.Combine(root, "configured");
        Directory.CreateDirectory(configuredDirectory);
        var expectedToolPath = Path.Combine(configuredDirectory, ToolExecutable("ffprobe"));
        await File.WriteAllTextAsync(expectedToolPath, "");

        var settingsStore = new AppSettingsStore(root, [root]);
        await settingsStore.SaveAsync(
            new AppSettings(FfprobePath: configuredDirectory),
            CancellationToken.None);
        var locator = new ExternalToolLocator(settingsStore, [], new FakeMkvToolNixInstallProbe());

        var location = await locator.LocateAsync("ffprobe", CancellationToken.None);

        Assert.True(location.Found);
        Assert.Equal(expectedToolPath, location.Path);
    }

    [Fact]
    public async Task LocateAsync_uses_configured_ffmpeg_directory_for_ffprobe()
    {
        var root = CreateTempDirectory();
        var ffmpegDirectory = Path.Combine(root, "ffmpeg");
        Directory.CreateDirectory(ffmpegDirectory);
        var expectedToolPath = Path.Combine(ffmpegDirectory, ToolExecutable("ffprobe"));
        await File.WriteAllTextAsync(expectedToolPath, "");

        var settingsStore = new AppSettingsStore(root, [root]);
        await settingsStore.SaveAsync(
            new AppSettings(FfmpegPath: ffmpegDirectory),
            CancellationToken.None);
        var locator = new ExternalToolLocator(settingsStore, [], new FakeMkvToolNixInstallProbe());

        var location = await locator.LocateAsync("ffprobe", CancellationToken.None);

        Assert.True(location.Found);
        Assert.Equal(expectedToolPath, location.Path);
    }

    [Fact]
    public async Task LocateAsync_uses_search_directory_for_ffprobe_when_unconfigured()
    {
        var root = CreateTempDirectory();
        var searchDirectory = Path.Combine(root, "search");
        Directory.CreateDirectory(searchDirectory);
        var expectedToolPath = Path.Combine(searchDirectory, ToolExecutable("ffprobe"));
        await File.WriteAllTextAsync(expectedToolPath, "");

        var locator = new ExternalToolLocator(new AppSettingsStore(root, [root]), [searchDirectory], new FakeMkvToolNixInstallProbe());

        var location = await locator.LocateAsync("ffprobe", CancellationToken.None);

        Assert.True(location.Found);
        Assert.Equal(expectedToolPath, location.Path);
    }

    [Fact]
    public void WindowsProbe_expands_registry_install_directories_and_display_icons()
    {
        var root = CreateTempDirectory();
        var installDirectory = Path.Combine(root, "MKVToolNix");
        Directory.CreateDirectory(installDirectory);
        var expectedToolPath = Path.Combine(installDirectory, ToolExecutable("mkvextract"));
        File.WriteAllText(expectedToolPath, "");
        var displayIcon = Path.Combine(installDirectory, OperatingSystem.IsWindows() ? "mkvtoolnix-gui.exe,0" : "mkvtoolnix-gui,0");
        var probe = new WindowsMkvToolNixInstallProbe(
            new FakeWindowsRegistryInstallProbe([installDirectory, displayIcon]),
            enabled: true);

        var candidates = probe.FindMkvExtractCandidates(ToolExecutable("mkvextract")).ToArray();

        Assert.Contains(expectedToolPath, candidates);
    }

    [Fact]
    public void MacProbe_finds_mkvextract_inside_versioned_app_bundle()
    {
        var root = CreateTempDirectory();
        var expectedToolPath = Path.Combine(root, "MKVToolNix-96.0.app", "Contents", "MacOS", "mkvextract");
        Directory.CreateDirectory(Path.GetDirectoryName(expectedToolPath)!);
        File.WriteAllText(expectedToolPath, "");
        var probe = new MacMkvToolNixInstallProbe([root], enabled: true);

        var candidates = probe.FindMkvExtractCandidates("mkvextract").ToArray();

        Assert.Contains(expectedToolPath, candidates);
    }

    [Fact]
    public void UnixProbe_returns_no_platform_install_candidates()
    {
        var probe = new UnixMkvToolNixInstallProbe();

        Assert.Empty(probe.FindMkvExtractCandidates("mkvextract"));
    }

    private static string ToolExecutable(string name) => OperatingSystem.IsWindows() ? $"{name}.exe" : name;

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "ChapterTool.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class FakeMkvToolNixInstallProbe(params string[] candidates) : IMkvToolNixInstallProbe
    {
        public IEnumerable<string> FindMkvExtractCandidates(string executableName) => candidates;
    }

    private sealed class FakeWindowsRegistryInstallProbe(IReadOnlyList<string> values) : IWindowsRegistryInstallProbe
    {
        public IEnumerable<string> ReadMkvToolNixInstallValues() => values;
    }
}
