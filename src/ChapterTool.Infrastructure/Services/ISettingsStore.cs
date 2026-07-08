namespace ChapterTool.Infrastructure.Services;

public interface ISettingsStore<TSettings>
{
    ValueTask<TSettings> LoadAsync(CancellationToken cancellationToken);

    ValueTask SaveAsync(TSettings settings, CancellationToken cancellationToken);
}
