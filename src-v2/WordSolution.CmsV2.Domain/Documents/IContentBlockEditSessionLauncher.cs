namespace WordSolution.CmsV2.Domain.Documents;

public interface IContentBlockEditSessionLauncher
{
    Task<ContentBlockEditLaunchResult> LaunchAsync(
        ContentBlockEditSession session,
        CancellationToken cancellationToken = default);
}

