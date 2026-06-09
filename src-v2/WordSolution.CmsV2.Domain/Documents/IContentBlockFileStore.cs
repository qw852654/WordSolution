namespace WordSolution.CmsV2.Domain.Documents;

public interface IContentBlockFileStore
{
    Task SaveContentBlockDocxAsync(
        string docxPath,
        Stream docxStream,
        CancellationToken cancellationToken = default);

    Task<byte[]?> ReadContentBlockDocxAsync(
        string docxPath,
        CancellationToken cancellationToken = default);

    Task<string?> ReadHtmlPreviewAsync(
        string htmlPreviewPath,
        CancellationToken cancellationToken = default);

    Task SavePlainTextAsync(
        string plainTextPath,
        string plainText,
        CancellationToken cancellationToken = default);

    Task<string?> ReadPlainTextAsync(
        string plainTextPath,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    Task DeleteIfExistsAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}
