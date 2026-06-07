using Avalonia;

namespace ChapterTool.Avalonia;

internal static class Program
{
    internal static IReadOnlyList<string> StartupArgs { get; private set; } = [];

    [STAThread]
    public static void Main(string[] args)
    {
        StartupArgs = args;
        if (args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase))
        {
            BuildAvaloniaApp();
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }
}
