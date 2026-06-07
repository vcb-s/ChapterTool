using ChapterTool.Core.Services;

namespace ChapterTool.Infrastructure.Platform;

public sealed class InMemoryApplicationLogService : IApplicationLogService
{
    private readonly List<ApplicationLogEntry> entries = [];

    public IReadOnlyList<ApplicationLogEntry> Entries => entries;

    public void Add(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        entries.Add(new ApplicationLogEntry(DateTimeOffset.Now, message.Trim()));
    }

    public string Format() =>
        string.Join(Environment.NewLine, entries.Select(static entry => $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] {entry.Message}"));

    public void Clear() => entries.Clear();
}
