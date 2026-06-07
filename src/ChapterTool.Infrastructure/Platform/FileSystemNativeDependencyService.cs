namespace ChapterTool.Infrastructure.Platform;

public sealed class FileSystemNativeDependencyService(IReadOnlyList<string> searchDirectories) : INativeDependencyService
{
    public ValueTask<NativeDependencyLocation> ResolveAsync(string dependencyId, CancellationToken cancellationToken)
    {
        foreach (var directory in searchDirectories)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var name in CandidateNames(dependencyId))
            {
                var path = Path.Combine(directory, name);
                if (File.Exists(path))
                {
                    return ValueTask.FromResult(new NativeDependencyLocation(true, path));
                }
            }
        }

        return ValueTask.FromResult(new NativeDependencyLocation(
            false,
            null,
            "NativeLibraryMissing",
            $"Native dependency '{dependencyId}' was not found."));
    }

    private static IReadOnlyList<string> CandidateNames(string dependencyId)
    {
        if (dependencyId.Equals("libmp4v2", StringComparison.OrdinalIgnoreCase))
        {
            if (OperatingSystem.IsWindows())
            {
                return ["libmp4v2.dll"];
            }

            if (OperatingSystem.IsMacOS())
            {
                return ["libmp4v2.dylib"];
            }

            return ["libmp4v2.so"];
        }

        return [dependencyId];
    }
}
