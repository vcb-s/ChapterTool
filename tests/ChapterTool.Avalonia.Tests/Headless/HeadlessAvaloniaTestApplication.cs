using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Skia;
using ChapterTool.Avalonia;

[assembly: AvaloniaTestApplication(typeof(ChapterTool.Avalonia.Tests.Headless.HeadlessAvaloniaTestApplication))]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerAssembly)]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace ChapterTool.Avalonia.Tests.Headless;

public static class HeadlessAvaloniaTestApplication
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder
            .Configure<App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .LogToTrace();
}
