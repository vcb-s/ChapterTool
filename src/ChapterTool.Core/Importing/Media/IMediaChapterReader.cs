namespace ChapterTool.Core.Importing.Media;

/// <summary>
/// Defines a reader for media container chapter metadata supplied by an integration layer.
/// </summary>
public interface IMediaChapterReader
{
    /// <summary>
    /// Reads chapter metadata from a media source path.
    /// </summary>
    /// <param name="path">The source media path.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The media chapter read result.</returns>
    ValueTask<MediaChapterReadResult> ReadAsync(string path, CancellationToken cancellationToken);
}
