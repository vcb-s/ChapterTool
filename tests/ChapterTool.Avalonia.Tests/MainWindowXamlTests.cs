using System.Xml.Linq;
using System.Text;

namespace ChapterTool.Avalonia.Tests;

public sealed class MainWindowXamlTests
{
    [Fact]
    public void MainWindowDeclaresInteractiveChapterToolSurface()
    {
        var text = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "ChapterTool.Avalonia", "Views", "MainWindow.axaml"), Encoding.UTF8);

        Assert.Contains("<DataGrid", text, StringComparison.Ordinal);
        Assert.Contains("<DataGrid.ContextMenu>", text, StringComparison.Ordinal);
        Assert.Contains("ProgressBar", text, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"AdvancedOptions\"", text, StringComparison.Ordinal);
        Assert.Contains("AppendMplsButton", text, StringComparison.Ordinal);
    }

    [Fact]
    public void MainWindowUsesResponsiveModernToolLayout()
    {
        var text = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "ChapterTool.Avalonia", "Views", "MainWindow.axaml"), Encoding.UTF8);

        Assert.Contains("Background=\"#f3f4f6\"", text, StringComparison.Ordinal);
        Assert.Contains("Width=\"920\"", text, StringComparison.Ordinal);
        Assert.Contains("Height=\"720\"", text, StringComparison.Ordinal);
        Assert.Contains("Button.primaryAction", text, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"HorizontalContentAlignment\" Value=\"Center\" />", text, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"VerticalContentAlignment\" Value=\"Center\" />", text, StringComparison.Ordinal);
        Assert.Contains("RowDefinitions=\"Auto,*,Auto\"", text, StringComparison.Ordinal);
        Assert.Contains("ColumnDefinitions=\"Auto,*,Auto\"", text, StringComparison.Ordinal);
        Assert.Contains("Content=\"载入\"", text, StringComparison.Ordinal);
        Assert.Contains("Content=\"保存\"", text, StringComparison.Ordinal);
        Assert.DoesNotContain("杞藉叆", text, StringComparison.Ordinal);
        Assert.DoesNotContain("淇濆瓨", text, StringComparison.Ordinal);
        Assert.Contains("Header=\"时间点\"", text, StringComparison.Ordinal);
        Assert.Contains("Header=\"章节名\"", text, StringComparison.Ordinal);
        Assert.Contains("Header=\"帧数\"", text, StringComparison.Ordinal);
        Assert.Contains("Text=\"保存格式\"", text, StringComparison.Ordinal);
        Assert.Contains("Text=\"XML语言\"", text, StringComparison.Ordinal);
        Assert.Contains("WindowStartupLocation=\"CenterScreen\"", text, StringComparison.Ordinal);
    }

    [Fact]
    public void MainWindowDoesNotUseAbsoluteCanvasLayout()
    {
        var text = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "ChapterTool.Avalonia", "Views", "MainWindow.axaml"), Encoding.UTF8);

        Assert.DoesNotContain("<Canvas", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Canvas.Left", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Canvas.Top", text, StringComparison.Ordinal);
    }

    [Fact]
    public void MainWindowProtectsGridAndNumericTextFromClipping()
    {
        var text = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "ChapterTool.Avalonia", "Views", "MainWindow.axaml"), Encoding.UTF8);

        Assert.Contains("x:Name=\"OrderShiftBox\"", text, StringComparison.Ordinal);
        Assert.Contains("MinWidth=\"124\"", text, StringComparison.Ordinal);
        Assert.Contains("Header=\"#\" Binding=\"{Binding Number}\" IsReadOnly=\"True\" Width=\"64\" MinWidth=\"56\"", text, StringComparison.Ordinal);
        Assert.Contains("Header=\"时间点\" Binding=\"{Binding TimeText}\" Width=\"170\" MinWidth=\"132\"", text, StringComparison.Ordinal);
        Assert.Contains("Header=\"章节名\" Binding=\"{Binding Name}\" Width=\"*\" MinWidth=\"180\"", text, StringComparison.Ordinal);
        Assert.Contains("Header=\"帧数\" Binding=\"{Binding FramesInfo}\" Width=\"150\" MinWidth=\"96\"", text, StringComparison.Ordinal);
    }

    [Fact]
    public void MainWindowKeepsStatusProgressVisibleAndUnclipped()
    {
        var text = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "ChapterTool.Avalonia", "Views", "MainWindow.axaml"), Encoding.UTF8);
        var document = XDocument.Parse(text);
        var progressBar = document.Descendants()
            .Single(static element => element.Attributes().Any(static attribute =>
                attribute.Name.LocalName == "Name" && attribute.Value == "ProgressBar"));

        Assert.Contains("ColumnDefinitions=\"*,Auto\"", text, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"ProgressBar\"", text, StringComparison.Ordinal);
        Assert.Contains("Width=\"180\"", text, StringComparison.Ordinal);
        Assert.Contains("Height=\"10\"", text, StringComparison.Ordinal);
        Assert.Contains("Margin=\"12,0,16,0\"", text, StringComparison.Ordinal);
        Assert.Contains("VerticalAlignment=\"Center\"", text, StringComparison.Ordinal);
        Assert.DoesNotContain(progressBar.Attributes(), static attribute =>
            attribute.Name.LocalName == "IsVisible" && attribute.Value == "False");
    }

    [Fact]
    public void MainWindowUsesStructuredAdvancedOptionsForResponsiveResizing()
    {
        var text = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "ChapterTool.Avalonia", "Views", "MainWindow.axaml"), Encoding.UTF8);
        var code = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "ChapterTool.Avalonia", "Views", "MainWindow.axaml.cs"), Encoding.UTF8);

        Assert.Contains("x:Name=\"AdvancedOptionsGrid\"", text, StringComparison.Ordinal);
        Assert.Contains("ColumnDefinitions=\"*,*\"", text, StringComparison.Ordinal);
        Assert.Contains("RowDefinitions=\"Auto,Auto,Auto\"", text, StringComparison.Ordinal);
        Assert.Contains("ColumnSpacing=\"18\"", text, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"AdvancedOptions\"", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ColumnDefinitions=\"Auto,180,Auto,Auto,Auto,Auto\"", text, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"FormatOptionsGroup\"", text, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"NamingOptionsGroup\"", text, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"OrderShiftOptionsGroup\"", text, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"XmlLanguageOptionsGroup\"", text, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"ExpressionOptionsGroup\"", text, StringComparison.Ordinal);
        Assert.Contains("ColumnDefinitions=\"84,*\"", text, StringComparison.Ordinal);
        Assert.Contains("ColumnDefinitions=\"84,124\"", text, StringComparison.Ordinal);
        Assert.Contains("ColumnDefinitions=\"Auto,Auto,*\"", text, StringComparison.Ordinal);
        Assert.Contains("HorizontalAlignment=\"Stretch\"", text, StringComparison.Ordinal);
        Assert.Contains("HorizontalAlignment=\"Right\"", text, StringComparison.Ordinal);
        Assert.Contains("MinWidth=\"240\"", text, StringComparison.Ordinal);
        Assert.Contains("MinWidth=\"300\"", text, StringComparison.Ordinal);
        Assert.Contains("MinWidth=\"78\"", text, StringComparison.Ordinal);
        Assert.Contains("SizeChanged += (_, _) => ApplyAdvancedOptionsLayout();", code, StringComparison.Ordinal);
        Assert.Contains("private void ApplyAdvancedOptionsLayout()", code, StringComparison.Ordinal);
        Assert.Contains("Bounds.Width >= 900", code, StringComparison.Ordinal);
        Assert.Contains("*,*,*", code, StringComparison.Ordinal);
        Assert.Contains("*,*", code, StringComparison.Ordinal);
    }

    [Fact]
    public void MainWindowDoesNotExposeRegistryDependentFileAssociationAsPrimaryUi()
    {
        var text = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "ChapterTool.Avalonia", "Views", "MainWindow.axaml"), Encoding.UTF8);

        Assert.DoesNotContain("FileAssociationButton", text, StringComparison.Ordinal);
        Assert.DoesNotContain("FileAssociationWindow", text, StringComparison.Ordinal);
        Assert.DoesNotContain("LanguageButton", text, StringComparison.Ordinal);
        Assert.DoesNotContain("LanguageWindow", text, StringComparison.Ordinal);
    }

    [Fact]
    public void MainWindowRefreshGuardsClipSelectionReentrancy()
    {
        var text = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "ChapterTool.Avalonia", "Views", "MainWindow.axaml.cs"), Encoding.UTF8);

        Assert.Contains("private bool isRefreshing", text, StringComparison.Ordinal);
        Assert.Contains("if (isRefreshing)", text, StringComparison.Ordinal);
        Assert.Contains("finally", text, StringComparison.Ordinal);
        Assert.Contains("isRefreshing = false", text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("SourcePath")]
    [InlineData("LoadButton")]
    [InlineData("SaveButton")]
    [InlineData("SaveToButton")]
    [InlineData("ChapterGrid")]
    [InlineData("ClipSelector")]
    [InlineData("OpenRelatedMediaButton")]
    [InlineData("AdvancedOptions")]
    [InlineData("PreviewWindow")]
    [InlineData("LogWindow")]
    public void MainWindowExposesStableAutomationIds(string automationId)
    {
        var document = XDocument.Load(Path.Combine(RepositoryRoot(), "src", "ChapterTool.Avalonia", "Views", "MainWindow.axaml"));
        var values = document.Descendants()
            .SelectMany(static element => element.Attributes())
            .Where(static attribute => attribute.Name.LocalName.EndsWith("AutomationId", StringComparison.Ordinal))
            .Select(static attribute => attribute.Value)
            .ToArray();

        Assert.Contains(automationId, values);
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
