using ChapterTool.Core.Diagnostics;

namespace ChapterTool.Core.Transform;

/// <summary>
/// Represents the result of expression evaluation.
/// </summary>
/// <param name="Success">The Success value.</param>
/// <param name="Value">The Value value.</param>
/// <param name="Diagnostics">The Diagnostics value.</param>
public sealed record ExpressionEvaluationResult(
    bool Success,
    decimal Value,
    IReadOnlyList<ChapterDiagnostic> Diagnostics);
