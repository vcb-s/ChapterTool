using ChapterTool.Core.Diagnostics;

namespace ChapterTool.Core.Exporting;

/// <summary>
/// Represents the result of a chapter conversion operation.
/// </summary>
/// <param name="Success">The Success value.</param>
/// <param name="Content">The Content value.</param>
/// <param name="Extension">The Extension value.</param>
/// <param name="Diagnostics">The Diagnostics value.</param>
public sealed record ChapterConversionResult(
    bool Success,
    string Content,
    string Extension,
    IReadOnlyList<ChapterDiagnostic> Diagnostics);
