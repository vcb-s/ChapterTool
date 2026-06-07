using System.Xml.Linq;

namespace ChapterTool.Avalonia.Tests;

public sealed class ResourcePackagingTests
{
    [Fact]
    public void RequiredAssetsExist()
    {
        var root = RepositoryRoot();
        Assert.True(File.Exists(Path.Combine(root, "src", "ChapterTool.Avalonia", "Assets", "Icons", "app-icon.svg")));
        Assert.True(File.Exists(Path.Combine(root, "src", "ChapterTool.Avalonia", "Assets", "Images", "chapter-empty.svg")));
    }

    [Fact]
    public void AssetsAreCopiedToBuildOutput()
    {
        var root = RepositoryRoot();
        var projectPath = Path.Combine(root, "src", "ChapterTool.Avalonia", "ChapterTool.Avalonia.csproj");
        var document = XDocument.Load(projectPath);
        var contentItems = document.Descendants("Content").ToArray();

        Assert.Contains(contentItems, item =>
            string.Equals((string?)item.Attribute("Include"), @"Assets\**\*", StringComparison.Ordinal) &&
            string.Equals((string?)item.Attribute("CopyToOutputDirectory"), "PreserveNewest", StringComparison.Ordinal));
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
