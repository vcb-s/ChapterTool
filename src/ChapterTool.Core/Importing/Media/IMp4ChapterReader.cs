namespace ChapterTool.Core.Importing.Media;

/// <summary>
/// Defines a reader for MP4 chapter metadata supplied by an integration layer.
/// </summary>
public interface IMp4ChapterReader
{
    /// <summary>
    /// Reads MP4 chapter metadata from a media source path.
    /// </summary>
    /// <param name="path">The source media path.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The MP4 chapter read result.</returns>
    ValueTask<Mp4ChapterReadResult> ReadAsync(string path, CancellationToken cancellationToken);
}
