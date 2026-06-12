namespace ChapterTool.Infrastructure.Configuration;

public sealed record AppSettings(
    string? SavingPath = null,
    string Language = "",
    WindowLocation? MainWindowLocation = null,
    string? MkvToolnixPath = null,
    string? Eac3toPath = null,
    string? FfprobePath = null,
    string? FfmpegPath = null,
    string DefaultSaveFormat = "Txt",
    string DefaultXmlLanguage = "und");

public sealed record WindowLocation(int X, int Y);
