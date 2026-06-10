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

    [Fact]
    public void Core_source_does_not_reference_platform_discovery_or_process_encoding_implementations()
    {
        var coreRoot = Path.Combine(FixtureResolver.RepositoryRoot, "src", "ChapterTool.Core");
        var sourceText = string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(coreRoot, "*.cs", SearchOption.AllDirectories)
                .Where(static path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                .Select(File.ReadAllText));

        Assert.DoesNotContain("Microsoft.Win32", sourceText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Registry", sourceText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("MkvToolNixInstallProbe", sourceText, StringComparison.Ordinal);
        Assert.DoesNotContain("MKVToolNix", sourceText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Contents", sourceText, StringComparison.Ordinal);
        Assert.DoesNotContain("MacOS", sourceText, StringComparison.Ordinal);
        Assert.DoesNotContain("StandardOutputEncoding", sourceText, StringComparison.Ordinal);
        Assert.DoesNotContain("StandardErrorEncoding", sourceText, StringComparison.Ordinal);
    }
}
