namespace WordSolution.CmsV2.Domain.Documents;

public interface IContentBlockEditSessionFileStore
{
    Task<string> CreateEditableDocxAsync(
        string bankRootDirectory,
        string sessionId,
        byte[] sourceDocx,
        CancellationToken cancellationToken = default);

    Task<byte[]?> ReadEditableDocxAsync(
        string editableDocxPath,
        CancellationToken cancellationToken = default);

    Task<string> ComputeHashAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    Task<bool> IsEditableDocxAvailableForSyncAsync(
        string editableDocxPath,
        CancellationToken cancellationToken = default);
}
