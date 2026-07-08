namespace ChapterTool.Core.Importing.Media;

/// <summary>
/// Represents the result returned by an MP4 chapter reader.
/// </summary>
/// <param name="Success">The Success value.</param>
/// <param name="Chapters">The Chapters value.</param>
/// <param name="DiagnosticCode">The DiagnosticCode value.</param>
/// <param name="Message">The Message value.</param>
public sealed record Mp4ChapterReadResult(
    bool Success,
    IReadOnlyList<Mp4ChapterClip> Chapters,
    string? DiagnosticCode = null,
    string? Message = null)
{
    /// <summary>
    /// Executes the Succeeded operation.
    /// </summary>
    /// <param name="chapters">The chapter entries.</param>
    /// <returns>The operation result.</returns>
    public static Mp4ChapterReadResult Succeeded(params Mp4ChapterClip[] chapters) => new(true, chapters);

    /// <summary>
    /// Executes the Failed operation.
    /// </summary>
    /// <param name="code">The diagnostic code.</param>
    /// <param name="message">The diagnostic message.</param>
    /// <returns>The operation result.</returns>
    public static Mp4ChapterReadResult Failed(string code, string message) => new(false, [], code, message);
}
