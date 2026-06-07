namespace ChapterTool.Infrastructure.Tests;

public sealed class InfrastructureBoundaryTests
{
    [Fact]
    public void Infrastructure_project_references_core()
    {
        var projectPath = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "ChapterTool.Infrastructure",
            "ChapterTool.Infrastructure.csproj");
        var projectText = File.ReadAllText(projectPath);

        Assert.Contains("ChapterTool.Core.csproj", projectText, StringComparison.Ordinal);
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
