namespace ChapterTool.Core.Importing.Cue;

/// <summary>
/// Imports chapter data from CUE sheet text files.
/// </summary>
public sealed class CueChapterImporter : IChapterImporter
{
    /// <summary>
    /// Gets the stable importer identifier.
    /// </summary>
    public string Id => "cue";

    /// <summary>
    /// Gets the supported file extensions for this importer.
    /// </summary>
    public IReadOnlySet<string> SupportedExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".cue"
    };

    /// <summary>
    /// Imports chapters from the supplied request.
    /// </summary>
    /// <param name="request">The import request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The operation result.</returns>
    public async ValueTask<ChapterImportResult> ImportAsync(ChapterImportRequest request, CancellationToken cancellationToken)
    {
        byte[] bytes;
        if (request.Content is not null)
        {
            using var memory = new MemoryStream();
            await request.Content.CopyToAsync(memory, cancellationToken);
            bytes = memory.ToArray();
        }
        else
        {
            bytes = await File.ReadAllBytesAsync(request.Path, cancellationToken);
        }

        var text = CueTextDecoder.Decode(bytes);
        return CueSheetParser.Parse(text, request.Path);
    }
}
