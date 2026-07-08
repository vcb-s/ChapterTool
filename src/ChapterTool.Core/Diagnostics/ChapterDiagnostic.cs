namespace ChapterTool.Core.Diagnostics;

/// <summary>
/// Represents a diagnostic message produced while importing, editing, transforming, or exporting chapters.
/// </summary>
/// <param name="Severity">The Severity value.</param>
/// <param name="Code">The Code value.</param>
/// <param name="Message">The Message value.</param>
/// <param name="Location">The Location value.</param>
/// <param name="Details">The Details value.</param>
/// <param name="Arguments">The Arguments value.</param>
public sealed record ChapterDiagnostic(
    DiagnosticSeverity Severity,
    string Code,
    string Message,
    string? Location = null,
    string? Details = null,
    IReadOnlyDictionary<string, object?>? Arguments = null);
