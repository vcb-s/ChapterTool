namespace ChapterTool.Infrastructure.Configuration;

public sealed record AppSettings(
    string? SavingPath = null,
    string Language = "",
    WindowLocation? MainWindowLocation = null,
    string? MkvToolnixPath = null,
    string? Eac3toPath = null);

public sealed record WindowLocation(int X, int Y);
