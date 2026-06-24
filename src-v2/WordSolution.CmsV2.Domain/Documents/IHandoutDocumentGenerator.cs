namespace WordSolution.CmsV2.Domain.Documents;

public sealed record HandoutDocumentSource(
    string Title,
    string DocxPath);

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
    string? OutputStemStyleName = null)
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
        string? outputStemStyleName = null)
    {
        return new HandoutDocumentElement(
            HandoutDocumentElementKind.ContentBlock,
            title,
            docxPath,
            HeadingLevel: 0,
            OutputStemStyleName: outputStemStyleName);
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
