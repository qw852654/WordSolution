namespace WordSolution.CmsV2.Domain.Documents;

public interface IContentBlockEditSessionStore
{
    Task SaveAsync(
        string bankRootDirectory,
        ContentBlockEditSession session,
        CancellationToken cancellationToken = default);

    Task<ContentBlockEditSession?> GetAsync(
        string bankRootDirectory,
        string sessionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentBlockEditSession>> ListActiveAsync(
        string bankRootDirectory,
        CancellationToken cancellationToken = default);
}
