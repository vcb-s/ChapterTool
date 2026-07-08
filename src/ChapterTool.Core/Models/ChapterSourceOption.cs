namespace ChapterTool.Core.Models;

/// <summary>
/// Represents one selectable chapter option within an imported source group.
/// </summary>
/// <param name="Id">The Id value.</param>
/// <param name="DisplayName">The DisplayName value.</param>
/// <param name="ChapterInfo">The ChapterInfo value.</param>
/// <param name="CanCombine">The CanCombine value.</param>
/// <param name="MediaReferences">The MediaReferences value.</param>
public sealed record ChapterSourceOption(
    string Id,
    string DisplayName,
    ChapterInfo ChapterInfo,
    bool CanCombine = false,
    IReadOnlyList<SourceMediaReference>? MediaReferences = null)
{
    /// <inheritdoc />
    public override string ToString() => DisplayName;
}
