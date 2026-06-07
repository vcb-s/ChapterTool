using System.Globalization;
using ChapterTool.Core.Models;

namespace ChapterTool.Core.Transform;

public sealed class FrameRateService : IFrameRateService
{
    private static readonly FrameRateOption[] FrameRateOptions =
    {
        new("Auto", "Auto", 0m, false, 0),
        new("Fps23976", "24000 / 1001", 24000m / 1001m, true, 1),
        new("Fps24", "24000 / 1000", 24m, true, 2),
        new("Fps25", "25000 / 1000", 25m, true, 3),
        new("Fps2997", "30000 / 1001", 30000m / 1001m, true, 4),
        new("Reserved", "RESER / VED", 0m, false, 5),
        new("Fps50", "50000 / 1000", 50m, true, 6),
        new("Fps5994", "60000 / 1001", 60000m / 1001m, true, 7),
    };

    public IReadOnlyList<FrameRateOption> Options => FrameRateOptions;

    public FrameRateOption FindByValue(decimal framesPerSecond)
    {
        return FrameRateOptions.FirstOrDefault(option =>
            option.IsValid && Math.Abs(option.Value - framesPerSecond) < 0.00001m)
            ?? FrameRateOptions[0];
    }

    public FrameRateOption Detect(ChapterInfo info, decimal tolerance)
    {
        var bestOption = FrameRateOptions[1];
        var bestScore = -1;

        foreach (var option in FrameRateOptions.Where(option => option.IsValid))
        {
            var score = info.Chapters.Sum(chapter => IsAccurate(chapter, option.Value, tolerance) ? 1 : 0);
            if (score > bestScore)
            {
                bestScore = score;
                bestOption = option;
            }
        }

        return bestOption;
    }

    public FrameInfoResult UpdateFrames(
        ChapterInfo info,
        FrameRateOption option,
        bool round,
        decimal tolerance)
    {
        var selectedOption = round && option.LegacyMplsCode == 0
            ? Detect(info, tolerance)
            : option;

        if (!selectedOption.IsValid)
        {
            selectedOption = FrameRateOptions[1];
        }

        var chapters = info.Chapters
            .Select(chapter => chapter with
            {
                FramesInfo = FormatFrames(chapter, selectedOption.Value, round, tolerance)
            })
            .ToArray();

        var updatedInfo = info with
        {
            FramesPerSecond = (double)selectedOption.Value,
            Chapters = chapters
        };

        return new FrameInfoResult(updatedInfo, chapters, selectedOption, selectedOption.Value);
    }

    private static bool IsAccurate(Chapter chapter, decimal framesPerSecond, decimal tolerance)
    {
        if (!framesPerSecondIsValid(framesPerSecond) || chapter.IsSeparator)
        {
            return false;
        }

        var frames = CalculateFrames(chapter, framesPerSecond);
        var rounded = Math.Round(frames, MidpointRounding.AwayFromZero);
        return Math.Abs(frames - rounded) < tolerance;

        static bool framesPerSecondIsValid(decimal fps) => fps > 0m;
    }

    private static string FormatFrames(Chapter chapter, decimal framesPerSecond, bool round, decimal tolerance)
    {
        var frames = CalculateFrames(chapter, framesPerSecond);
        if (!round)
        {
            return frames.ToString(CultureInfo.InvariantCulture);
        }

        var rounded = Math.Round(frames, MidpointRounding.AwayFromZero);
        var marker = Math.Abs(frames - rounded) < tolerance ? "K" : "*";
        return $"{rounded.ToString(CultureInfo.InvariantCulture)} {marker}";
    }

    private static decimal CalculateFrames(Chapter chapter, decimal framesPerSecond)
    {
        return (decimal)chapter.Time.TotalSeconds * framesPerSecond;
    }
}
