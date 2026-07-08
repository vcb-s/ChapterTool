namespace ChapterTool.Core.Importing;

/// <summary>
/// Reports chapter loading progress.
/// </summary>
/// <param name="Value">The Value value.</param>
/// <param name="Message">The Message value.</param>
public sealed record ChapterLoadProgress(double Value, string? Message = null);
