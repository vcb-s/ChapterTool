using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ChapterTool.Avalonia.Views;

namespace ChapterTool.Avalonia;

public sealed class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var startupPath = Program.StartupArgs.FirstOrDefault(static arg => !arg.StartsWith("--", StringComparison.Ordinal));
            desktop.MainWindow = new MainWindow(startupPath);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
