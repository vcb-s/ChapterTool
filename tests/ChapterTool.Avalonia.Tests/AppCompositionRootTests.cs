using ChapterTool.Avalonia.Composition;
using ChapterTool.Avalonia.Services;
using ChapterTool.Avalonia.ViewModels;
using ChapterTool.Core.Services;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ChapterTool.Avalonia.Tests;

public sealed class AppCompositionRootTests
{
    [Fact]
    public void ResolvesPrimaryViewModelsAndServices()
    {
        using var root = new AppCompositionRoot(settingsDirectory: SettingsDirectory());

        Assert.IsType<MainWindowViewModel>(root.CreateMainWindowViewModel());
        Assert.IsAssignableFrom<IWindowService>(root.CreateWindowService());
        Assert.IsAssignableFrom<IChapterLoadService>(root.CreateChapterLoadService());
        Assert.IsAssignableFrom<IChapterSaveService>(root.CreateChapterSaveService());
        Assert.IsAssignableFrom<IChapterImporterRegistry>(root.CreateChapterImporterRegistry());
        Assert.IsAssignableFrom<IApplicationLogService>(root.CreateApplicationLogService());
    }

    [Fact]
    public void RegistersLoggerPipelineUiSinkAndFileLogging()
    {
        var settingsDirectory = SettingsDirectory();
        var root = new AppCompositionRoot(settingsDirectory: settingsDirectory);
        var logger = root.CreateLogger<AppCompositionRootTests>();
        var state = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["MessageKey"] = "Log.Status",
            ["status"] = "Ready"
        };

        try
        {
            logger.Log(LogLevel.Information, new EventId(0, "Log.Status"), state, null, static (values, _) => values["MessageKey"]?.ToString() ?? string.Empty);
            logger.LogError("composition-file-log-check");

            var logService = root.CreateApplicationLogService();
            Assert.Contains(logService.Entries, entry =>
                entry.MessageKey == "Log.Status" &&
                entry.Level == LogLevel.Information &&
                string.Equals(entry.Category, typeof(AppCompositionRootTests).FullName, StringComparison.Ordinal));
        }
        finally
        {
            root.Dispose();
        }

        var logDirectory = Path.Combine(settingsDirectory, "logs");
        var logFile = Assert.Single(Directory.EnumerateFiles(logDirectory, "chaptertool-*.log"));
        var logText = File.ReadAllText(logFile, Encoding.UTF8);
        Assert.Contains("composition-file-log-check", logText, StringComparison.Ordinal);

        Directory.Delete(settingsDirectory, recursive: true);
    }

    [Fact]
    public void StartupAndMainWindowDelegateCompositionToRoot()
    {
        var app = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "ChapterTool.Avalonia", "App.axaml.cs"), Encoding.UTF8);
        var mainWindow = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "ChapterTool.Avalonia", "Views", "MainWindow.axaml.cs"), Encoding.UTF8);

        Assert.Contains("new AppCompositionRoot(startupPath)", app, StringComparison.Ordinal);
        Assert.Contains("composition.CreateMainWindow()", app, StringComparison.Ordinal);
        Assert.DoesNotContain("new MainWindow(startupPath)", app, StringComparison.Ordinal);
        Assert.DoesNotContain("new AppSettingsStore", mainWindow, StringComparison.Ordinal);
        Assert.DoesNotContain("new ThemeSettingsStore", mainWindow, StringComparison.Ordinal);
        Assert.DoesNotContain("new RuntimeChapterLoadService", mainWindow, StringComparison.Ordinal);
        Assert.DoesNotContain("new RuntimeChapterSaveService", mainWindow, StringComparison.Ordinal);
    }

    [Fact]
    public void AppDisablesSystemDarkThemeFollowing()
    {
        var app = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "ChapterTool.Avalonia", "App.axaml"), Encoding.UTF8);

        Assert.Contains("RequestedThemeVariant=\"Light\"", app, StringComparison.Ordinal);
        Assert.DoesNotContain("RequestedThemeVariant=\"Default\"", app, StringComparison.Ordinal);
        Assert.DoesNotContain("RequestedThemeVariant=\"Dark\"", app, StringComparison.Ordinal);
    }

    private static string SettingsDirectory() =>
        Path.Combine(Path.GetTempPath(), "ChapterTool.Tests", Guid.NewGuid().ToString("N"));

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Time_Shift.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test output directory.");
    }
}
