using ChapterTool.Core.Diagnostics;

namespace ChapterTool.Core.Exporting;

/// <summary>
/// Represents the result of a chapter export operation.
/// </summary>
/// <param name="Success">The Success value.</param>
/// <param name="Content">The Content value.</param>
/// <param name="FileExtension">The FileExtension value.</param>
/// <param name="Diagnostics">The Diagnostics value.</param>
public sealed record ChapterExportResult(
    bool Success,
    string Content,
    string FileExtension,
    IReadOnlyList<ChapterDiagnostic> Diagnostics);
