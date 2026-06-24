using WordSolution.CmsV2.Domain.Documents;

namespace WordSolution.CmsV2.Infrastructure.Documents;

public sealed class LocalQuestionImportDocumentCloseChecker : IQuestionImportDocumentCloseChecker
{
    public Task<bool> IsClosedAsync(
        string sourceDocxPath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(sourceDocxPath) || !File.Exists(sourceDocxPath))
        {
            return Task.FromResult(false);
        }

        try
        {
            using var stream = new FileStream(
                sourceDocxPath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None);
            return Task.FromResult(true);
        }
        catch (IOException)
        {
            return Task.FromResult(false);
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult(false);
        }
    }
}
