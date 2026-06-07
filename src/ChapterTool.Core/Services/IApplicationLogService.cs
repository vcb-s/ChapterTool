namespace ChapterTool.Core.Services;

public interface IApplicationLogService
{
    IReadOnlyList<ApplicationLogEntry> Entries { get; }

    void Add(string message);

    string Format();

    void Clear();
}

public sealed record ApplicationLogEntry(DateTimeOffset Timestamp, string Message);
