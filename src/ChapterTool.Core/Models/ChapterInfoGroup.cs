namespace ChapterTool.Core.Models;

/// <summary>
/// Represents one source path and the chapter options discovered for that source.
/// </summary>
/// <param name="SourcePath">The SourcePath value.</param>
/// <param name="Options">The Options value.</param>
/// <param name="DefaultOptionIndex">The DefaultOptionIndex value.</param>
public sealed record ChapterInfoGroup(
    string SourcePath,
    IReadOnlyList<ChapterSourceOption> Options,
    int DefaultOptionIndex = 0);
