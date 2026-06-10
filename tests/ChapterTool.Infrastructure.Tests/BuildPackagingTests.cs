using System.Xml.Linq;

namespace ChapterTool.Infrastructure.Tests;

public sealed class BuildPackagingTests
{
    [Fact]
    public void CiWorkflowUsesDotNetCliForNet10BuildAndTest()
    {
        var workflow = File.ReadAllText(Path.Combine(RepositoryRoot(), ".github", "workflows", "dotnet-ci.yml"));

        Assert.Contains("dotnet-version: 10.0.x", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet restore ChapterTool.Avalonia.slnx", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet build ChapterTool.Avalonia.slnx", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet test ChapterTool.Avalonia.slnx", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void PublishScriptDefinesExplicitArtifacts()
    {
        var script = File.ReadAllText(Path.Combine(RepositoryRoot(), "scripts", "publish.ps1"));

        Assert.Contains("dotnet publish", script, StringComparison.Ordinal);
        Assert.Contains("artifacts/publish", script, StringComparison.Ordinal);
        Assert.Contains("self-contained", script, StringComparison.Ordinal);
        Assert.Contains("framework-dependent", script, StringComparison.Ordinal);
    }

    [Fact]
    public void VersionComesFromDirectoryBuildProps()
    {
        var document = XDocument.Load(Path.Combine(RepositoryRoot(), "Directory.Build.props"));

        Assert.Equal("0.1.0", document.Descendants("VersionPrefix").Single().Value);
        foreach (var project in Directory.EnumerateFiles(Path.Combine(RepositoryRoot(), "src"), "*.csproj", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(project);
            Assert.DoesNotContain("<Version>", text, StringComparison.Ordinal);
            Assert.DoesNotContain("<VersionPrefix>", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void PackagingStrategyDocumentsMp4ManagedDependencyAndInstallerPolicy()
    {
        var document = File.ReadAllText(Path.Combine(RepositoryRoot(), "docs", "packaging-strategy.md"));

        Assert.Contains("IMp4ChapterReader", document, StringComparison.Ordinal);
        Assert.Contains("ATL.NET", document, StringComparison.Ordinal);
        Assert.Contains("z440.atl.core", document, StringComparison.Ordinal);
        Assert.Contains("No separate MP4 command-line tool or `libmp4v2` DLL is required", document, StringComparison.Ordinal);
        Assert.DoesNotContain("NativeLibraryMissing", document, StringComparison.Ordinal);
        Assert.Contains("Fody and Costura are retired", document, StringComparison.Ordinal);
        Assert.Contains("legacy NSIS/Costura packaging path is not carried forward", document, StringComparison.Ordinal);
    }

    [Fact]
    public void AvaloniaProjectDoesNotBundleLegacyMp4NativeDlls()
    {
        var project = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "ChapterTool.Avalonia", "ChapterTool.Avalonia.csproj"));
        var publishScript = File.ReadAllText(Path.Combine(RepositoryRoot(), "scripts", "publish.ps1"));

        Assert.DoesNotContain("Time_Shift/mp4v2", project, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Time_Shift\\mp4v2", project, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("libmp4v2", project, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Time_Shift/mp4v2", publishScript, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Time_Shift\\mp4v2", publishScript, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("libmp4v2", publishScript, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReadmeDocumentsAvaloniaNet10FormatsAndDependencies()
    {
        var readme = File.ReadAllText(Path.Combine(RepositoryRoot(), "README.md"));

        Assert.Contains(".NET 10", readme, StringComparison.Ordinal);
        Assert.Contains(".cue", readme, StringComparison.Ordinal);
        Assert.Contains("eac3to", readme, StringComparison.Ordinal);
        Assert.Contains("dotnet test ChapterTool.Avalonia.slnx", readme, StringComparison.Ordinal);
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
}
