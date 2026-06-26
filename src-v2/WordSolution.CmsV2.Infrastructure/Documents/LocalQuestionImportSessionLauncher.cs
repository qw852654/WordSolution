using WordSolution.CmsV2.Domain.Documents;

namespace WordSolution.CmsV2.Infrastructure.Documents;

public sealed class LocalQuestionImportSessionLauncher : IQuestionImportSessionLauncher
{
    public Task OpenAsync(
        QuestionImportSessionLaunchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.SourceDocxPath))
        {
            throw new ArgumentException("Question import source DOCX path cannot be empty.", nameof(request));
        }

        if (!File.Exists(request.SourceDocxPath))
        {
            throw new FileNotFoundException("The question import source DOCX file was not found.", request.SourceDocxPath);
        }

        LocalWordDocumentOpener.Open(request.SourceDocxPath);

        return Task.CompletedTask;
    }
}
