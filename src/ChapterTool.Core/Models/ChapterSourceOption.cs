namespace ChapterTool.Core.Models;

public sealed record ChapterSourceOption(
    string Id,
    string DisplayName,
    ChapterInfo ChapterInfo,
    bool CanCombine = false,
    IReadOnlyList<SourceMediaReference>? MediaReferences = null);
