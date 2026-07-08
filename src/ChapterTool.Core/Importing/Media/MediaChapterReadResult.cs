namespace ChapterTool.Core.Importing.Media;

/// <summary>
/// Represents the result returned by a media chapter reader.
/// </summary>
/// <param name="Success">The Success value.</param>
/// <param name="Chapters">The Chapters value.</param>
/// <param name="DiagnosticCode">The DiagnosticCode value.</param>
/// <param name="Message">The Message value.</param>
/// <param name="Details">The Details value.</param>
public sealed record MediaChapterReadResult(
    bool Success,
    IReadOnlyList<MediaChapterEntry> Chapters,
    string? DiagnosticCode = null,
    string? Message = null,
    string? Details = null)
{
    /// <summary>
    /// Executes the Succeeded operation.
    /// </summary>
    /// <param name="chapters">The chapter entries.</param>
    /// <returns>The operation result.</returns>
    public static MediaChapterReadResult Succeeded(params MediaChapterEntry[] chapters) => new(true, chapters);

    /// <summary>
    /// Executes the Failed operation.
    /// </summary>
    /// <param name="code">The diagnostic code.</param>
    /// <param name="message">The diagnostic message.</param>
    /// <param name="details">Additional diagnostic details.</param>
    /// <returns>The operation result.</returns>
    public static MediaChapterReadResult Failed(string code, string message, string? details = null) =>
        new(false, [], code, message, details);
}
