namespace ChapterTool.Infrastructure.Services;

public interface IProcessRunner
{
    ValueTask<ProcessRunResult> RunAsync(ProcessRunRequest request, CancellationToken cancellationToken);
}
