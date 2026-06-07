namespace ChapterTool.Avalonia.Tests;

public sealed class AvaloniaProjectBoundaryTests
{
    [Fact]
    public void Avalonia_project_references_core_and_infrastructure()
    {
        var projectPath = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "ChapterTool.Avalonia",
            "ChapterTool.Avalonia.csproj");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("ChapterTool.Core.csproj", projectText, StringComparison.Ordinal);
        Assert.Contains("ChapterTool.Infrastructure.csproj", projectText, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
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

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
