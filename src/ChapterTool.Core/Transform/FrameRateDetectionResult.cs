namespace ChapterTool.Core.Transform;

/// <summary>
/// Represents detailed frame rate detection output.
/// </summary>
/// <param name="Option">The Option value.</param>
/// <param name="AccurateChapterCount">The AccurateChapterCount value.</param>
/// <param name="EvaluatedChapterCount">The EvaluatedChapterCount value.</param>
/// <param name="CumulativeDeviation">The CumulativeDeviation value.</param>
/// <param name="Confidence">The Confidence value.</param>
public sealed record FrameRateDetectionResult(
    FrameRateOption Option,
    int AccurateChapterCount,
    int EvaluatedChapterCount,
    decimal CumulativeDeviation,
    FrameRateConfidence Confidence);
