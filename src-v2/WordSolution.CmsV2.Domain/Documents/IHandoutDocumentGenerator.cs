namespace WordSolution.CmsV2.Domain.Documents;

public sealed record HandoutDocumentSource(
    string Title,
    string DocxPath);

public enum HandoutDocumentElementKind
{
    Heading = 1,
    ContentBlock = 2
}

public sealed record HandoutDocumentElement(
    HandoutDocumentElementKind Kind,
    string Title,
    string? DocxPath,
    int HeadingLevel)
{
    public static HandoutDocumentElement Heading(string title, int headingLevel)
    {
        return new HandoutDocumentElement(
            HandoutDocumentElementKind.Heading,
            title,
            DocxPath: null,
            headingLevel);
    }

    public static HandoutDocumentElement ContentBlock(string title, string docxPath)
    {
        return new HandoutDocumentElement(
            HandoutDocumentElementKind.ContentBlock,
            title,
            docxPath,
            HeadingLevel: 0);
    }
}

public interface IHandoutDocumentGenerator
{
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
