namespace WordSolution.CmsV2.Domain.Documents;

public sealed record HandoutDocumentSource(
    string Title,
    string DocxPath);

public sealed record HandoutDocumentGenerationIssue(
    string Code,
    string Message,
    int? OutputFormId = null,
    int? ContentBlockId = null,
    int? ContentBlockVersionId = null,
    int? OutputTemplateId = null,
    string? RequiredStyleName = null,
    string? OccurrenceRole = null);

public sealed class HandoutDocumentGenerationException : Exception
{
    public HandoutDocumentGenerationException(string message)
        : base(message)
    {
    }

    public HandoutDocumentGenerationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public enum HandoutDocumentElementKind
{
    Heading = 1,
    ContentBlock = 2
}

public sealed record HandoutDocumentElement(
    HandoutDocumentElementKind Kind,
    string Title,
    string? DocxPath,
    int HeadingLevel,
    string? OutputStemStyleName = null,
    int? ContentBlockId = null,
    int? ContentBlockVersionId = null,
    string? OccurrenceRole = null)
{
    public static HandoutDocumentElement Heading(string title, int headingLevel)
    {
        return new HandoutDocumentElement(
            HandoutDocumentElementKind.Heading,
            title,
            DocxPath: null,
            headingLevel);
    }

    public static HandoutDocumentElement ContentBlock(
        string title,
        string docxPath,
        string? outputStemStyleName = null,
        int? contentBlockId = null,
        int? contentBlockVersionId = null,
        string? occurrenceRole = null)
    {
        return new HandoutDocumentElement(
            HandoutDocumentElementKind.ContentBlock,
            title,
            docxPath,
            HeadingLevel: 0,
            OutputStemStyleName: outputStemStyleName,
            ContentBlockId: contentBlockId,
            ContentBlockVersionId: contentBlockVersionId,
            OccurrenceRole: occurrenceRole);
    }
}

public interface IHandoutDocumentGenerator
{
    Task<IReadOnlyList<HandoutDocumentGenerationIssue>> ValidateWordGenerationAsync(
        string templateDocxPath,
        IReadOnlyList<HandoutDocumentElement> elements,
        CancellationToken cancellationToken = default);

    Task GenerateWordAsync(
        string handoutTitle,
        string templateDocxPath,
        IReadOnlyList<HandoutDocumentSource> sources,
        string outputDocxPath,
        DateTimeOffset generatedTime,
        CancellationToken cancellationToken = default);

    Task GenerateWordAsync(
        string handoutTitle,
        string templateDocxPath,
        IReadOnlyList<HandoutDocumentElement> elements,
        string outputDocxPath,
        DateTimeOffset generatedTime,
        CancellationToken cancellationToken = default);
}
