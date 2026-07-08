using ChapterTool.Core.Diagnostics;
using ChapterTool.Core.Models;

namespace ChapterTool.Core.Editing;

/// <summary>
/// Represents the result of a chapter edit operation.
/// </summary>
/// <param name="ChapterInfo">The ChapterInfo value.</param>
/// <param name="Diagnostics">The Diagnostics value.</param>
public sealed record ChapterEditResult(
    ChapterInfo ChapterInfo,
    IReadOnlyList<ChapterDiagnostic> Diagnostics);
