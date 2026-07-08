namespace ChapterTool.Core.Importing;

/// <summary>
/// Defines a chapter importer for one or more source formats.
/// </summary>
public interface IChapterImporter
{
    /// <summary>
    /// Gets the stable importer identifier.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the supported file extensions for this importer.
    /// </summary>
    IReadOnlySet<string> SupportedExtensions { get; }

    /// <summary>
    /// Imports chapters from the supplied request.
    /// </summary>
    /// <param name="request">The import request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The import result.</returns>
    ValueTask<ChapterImportResult> ImportAsync(ChapterImportRequest request, CancellationToken cancellationToken);
}
