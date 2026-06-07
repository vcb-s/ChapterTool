using ChapterTool.Core.Diagnostics;
using ChapterTool.Core.Models;

namespace ChapterTool.Core.Importing;

public sealed record ChapterImportResult(
    bool Success,
    IReadOnlyList<ChapterInfoGroup> Groups,
    IReadOnlyList<ChapterDiagnostic> Diagnostics,
    bool IsPartial = false)
{
    public static ChapterImportResult Succeeded(params ChapterInfoGroup[] groups) =>
        new(true, groups, Array.Empty<ChapterDiagnostic>());

    public static ChapterImportResult Failed(params ChapterDiagnostic[] diagnostics) =>
        new(false, Array.Empty<ChapterInfoGroup>(), diagnostics);
}
