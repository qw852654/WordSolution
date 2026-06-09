namespace WordSolution.CmsV2.Domain.Documents;

public sealed record HandoutDocumentSource(
    string Title,
    string DocxPath);

public interface IHandoutDocumentGenerator
{
    Task GenerateWordAsync(
        string handoutTitle,
        string templateDocxPath,
        IReadOnlyList<HandoutDocumentSource> sources,
        string outputDocxPath,
        DateTimeOffset generatedTime,
        CancellationToken cancellationToken = default);
}
