using System.Diagnostics;
using ChapterTool.Avalonia;

namespace ChapterTool.Avalonia.Tests;

public sealed class ExecutableLaunchTests
{
    [Fact]
    public void SmokeTestArgumentBuildsAvaloniaAppAndExits()
    {
        var result = RunExecutable("--smoke-test", expectLongRunning: false);

        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public void ExecutableStartsMainWindowAndDoesNotExitImmediately()
    {
        var result = RunExecutable("", expectLongRunning: true);

        Assert.True(result.WasStillRunning, "The GUI process exited immediately instead of staying alive with a main window.");
    }

    [Fact]
    public void ExecutableStartsWithIfoFixtureAndDoesNotCrash()
    {
        var ifoPath = Path.Combine(RepositoryRoot(), "Time_Shift_Test", "[ifo_Sample]", "VTS_05_0.IFO");
        var result = RunExecutable($"\"{ifoPath}\"", expectLongRunning: true);

        Assert.True(result.WasStillRunning, "The GUI process crashed while loading an IFO startup path.");
        Assert.True(result.WorkingSetBytes < 256L * 1024 * 1024, $"Unexpected working set while loading IFO: {result.WorkingSetBytes} bytes.");
    }

    private static LaunchResult RunExecutable(string arguments, bool expectLongRunning)
    {
        var assembly = typeof(App).Assembly.Location;
        var executable = Path.ChangeExtension(assembly, OperatingSystem.IsWindows() ? ".exe" : null);
        Assert.True(File.Exists(executable), $"Expected executable next to test output: {executable}");

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            UseShellExecute = false
        });
        Assert.NotNull(process);

        if (!expectLongRunning)
        {
            Assert.True(process.WaitForExit(5000), "Smoke-test process did not exit.");
            return new LaunchResult(process.ExitCode, WasStillRunning: false);
        }

        Thread.Sleep(1500);
        var stillRunning = !process.HasExited;
        long workingSet = 0;
        if (stillRunning)
        {
            process.Refresh();
            workingSet = process.WorkingSet64;
            process.CloseMainWindow();
            if (!process.WaitForExit(2000))
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(2000);
            }
        }

        return new LaunchResult(process.HasExited ? process.ExitCode : null, stillRunning, workingSet);
    }

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

    private sealed record LaunchResult(int? ExitCode, bool WasStillRunning, long WorkingSetBytes = 0);
}
