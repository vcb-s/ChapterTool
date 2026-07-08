namespace ChapterTool.Core.Exporting;

/// <summary>
/// Identifies a supported chapter export format.
/// </summary>
public enum ChapterExportFormat
{
    /// <summary>
    /// Exports OGM-style chapter text.
    /// </summary>
    Txt,
    /// <summary>
    /// Exports Matroska XML chapters.
    /// </summary>
    Xml,
    /// <summary>
    /// Exports QPFile frame markers.
    /// </summary>
    Qpfile,
    /// <summary>
    /// Exports timestamp lines.
    /// </summary>
    TimeCodes,
    /// <summary>
    /// Exports tsMuxeR metadata.
    /// </summary>
    TsMuxerMeta,
    /// <summary>
    /// Exports a CUE sheet.
    /// </summary>
    Cue,
    /// <summary>
    /// Exports JSON chapter data.
    /// </summary>
    Json,
    /// <summary>
    /// Exports WebVTT cues.
    /// </summary>
    WebVtt,
    /// <summary>
    /// Exports celltimes frame numbers.
    /// </summary>
    Celltimes,
    /// <summary>
    /// Converts chapter text to QPFile output.
    /// </summary>
    Chapter2Qpfile
}
