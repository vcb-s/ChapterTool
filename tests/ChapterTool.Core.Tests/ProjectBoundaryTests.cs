using System.Xml.Linq;

namespace ChapterTool.Core.Tests;

public sealed class ProjectBoundaryTests
{
    [Fact]
    public void Core_project_does_not_reference_ui_or_windows_projects()
    {
        var projectPath = Path.Combine(
            FixtureResolver.RepositoryRoot,
            "src",
            "ChapterTool.Core",
            "ChapterTool.Core.csproj");
        var document = XDocument.Load(projectPath);
        var projectText = document.ToString(SaveOptions.DisableFormatting);

        Assert.DoesNotContain("Avalonia", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("System.Windows.Forms", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("System.Drawing", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Microsoft.Win32.Registry", projectText, StringComparison.OrdinalIgnoreCase);
    }
}
