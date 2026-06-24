namespace WordSolution.CmsV2.Domain.Documents;

public interface IContentBlockDocumentProcessor
{
    Task CreateBlankDocxAsync(
        string docxPath,
        CancellationToken cancellationToken = default);

    Task GenerateHtmlPreviewAsync(
        string docxPath,
        string htmlPreviewPath,
        CancellationToken cancellationToken = default);

    Task<string> ExtractPlainTextAsync(
        string docxPath,
        CancellationToken cancellationToken = default);

    Task<QuestionPartParseResult> GenerateQuestionPartHtmlPreviewAsync(
        string docxPath,
        string htmlPreviewPath,
        CancellationToken cancellationToken = default);
}
