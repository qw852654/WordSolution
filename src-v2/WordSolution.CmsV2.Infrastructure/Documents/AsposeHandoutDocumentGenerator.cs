using Aspose.Words;
using Aspose.Words.Lists;
using WordSolution.CmsV2.Domain.Documents;

namespace WordSolution.CmsV2.Infrastructure.Documents;

public sealed class AsposeHandoutDocumentGenerator : IHandoutDocumentGenerator
{
    public Task GenerateWordAsync(
        string handoutTitle,
        string templateDocxPath,
        IReadOnlyList<HandoutDocumentSource> sources,
        string outputDocxPath,
        DateTimeOffset generatedTime,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sources);

        var elements = sources
            .Select(source => HandoutDocumentElement.ContentBlock(source.Title, source.DocxPath))
            .ToArray();

        return GenerateWordAsync(
            handoutTitle,
            templateDocxPath,
            elements,
            outputDocxPath,
            generatedTime,
            cancellationToken);
    }

    public Task GenerateWordAsync(
        string handoutTitle,
        string templateDocxPath,
        IReadOnlyList<HandoutDocumentElement> elements,
        string outputDocxPath,
        DateTimeOffset generatedTime,
        CancellationToken cancellationToken = default)
    {
        ValidateText(handoutTitle, nameof(handoutTitle));
        ValidatePath(templateDocxPath, nameof(templateDocxPath));
        ArgumentNullException.ThrowIfNull(elements);
        ValidatePath(outputDocxPath, nameof(outputDocxPath));

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureParentDirectory(outputDocxPath);

            var outputDocument = new Document(templateDocxPath);
            var builder = new DocumentBuilder(outputDocument);

            builder.MoveToDocumentEnd();
            builder.InsertBreak(BreakType.ParagraphBreak);
            builder.Font.Name = "Microsoft YaHei";
            builder.Font.Size = 16;
            builder.Font.Bold = true;
            builder.Writeln(handoutTitle.Trim());
            builder.Font.Bold = false;
            builder.Font.Size = 10;
            builder.Writeln($"Generated: {generatedTime:yyyy-MM-dd HH:mm}");
            builder.Font.Size = 11;

            if (elements.Count == 0)
            {
                builder.Writeln("No content.");
            }

            foreach (var element in elements)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (element.Kind == HandoutDocumentElementKind.Heading)
                {
                    ValidateText(element.Title, nameof(element.Title));
                    WriteHeading(builder, element.Title, element.HeadingLevel);
                    continue;
                }

                if (element.Kind != HandoutDocumentElementKind.ContentBlock)
                {
                    throw new ArgumentException($"Unsupported handout document element kind: {element.Kind}.");
                }

                ValidatePath(element.DocxPath ?? string.Empty, nameof(element.DocxPath));

                var sourceDocument = new Document(element.DocxPath!);
                RemoveHeadersAndFooters(sourceDocument);
                outputDocument.AppendDocument(sourceDocument, ImportFormatMode.KeepSourceFormatting);
            }

            RebaseTopLevelNumberedParagraphs(outputDocument);
            outputDocument.Save(outputDocxPath, SaveFormat.Docx);
        }, cancellationToken);
    }

    private static void WriteHeading(DocumentBuilder builder, string title, int headingLevel)
    {
        if (headingLevel is < 1 or > 9)
        {
            throw new ArgumentOutOfRangeException(nameof(headingLevel), "Heading level must be between 1 and 9.");
        }

        builder.MoveToDocumentEnd();
        builder.InsertBreak(BreakType.ParagraphBreak);
        builder.ParagraphFormat.StyleIdentifier = GetHeadingStyleIdentifier(headingLevel);
        builder.Font.Name = "Microsoft YaHei";
        builder.Font.Bold = true;
        builder.Writeln(title.Trim());
        builder.Font.Bold = false;
        builder.ParagraphFormat.ClearFormatting();
        builder.Font.ClearFormatting();
    }

    private static StyleIdentifier GetHeadingStyleIdentifier(int headingLevel)
    {
        return headingLevel switch
        {
            1 => StyleIdentifier.Heading1,
            2 => StyleIdentifier.Heading2,
            3 => StyleIdentifier.Heading3,
            4 => StyleIdentifier.Heading4,
            5 => StyleIdentifier.Heading5,
            6 => StyleIdentifier.Heading6,
            7 => StyleIdentifier.Heading7,
            8 => StyleIdentifier.Heading8,
            _ => StyleIdentifier.Heading9
        };
    }

    private static void RebaseTopLevelNumberedParagraphs(Document document)
    {
        var numberedParagraphs = document
            .GetChildNodes(NodeType.Paragraph, true)
            .OfType<Paragraph>()
            .Where(paragraph => paragraph.IsListItem && paragraph.ListFormat.ListLevelNumber == 0)
            .ToArray();

        if (numberedParagraphs.Length == 0)
        {
            return;
        }

        var sharedList = document.Lists.Add(ListTemplate.NumberDefault);
        foreach (var paragraph in numberedParagraphs)
        {
            paragraph.ListFormat.List = sharedList;
            paragraph.ListFormat.ListLevelNumber = 0;
        }

        document.UpdateListLabels();
    }

    private static void RemoveHeadersAndFooters(Document document)
    {
        foreach (Section section in document.Sections)
        {
            section.HeadersFooters.Clear();
        }
    }

    private static void EnsureParentDirectory(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static void ValidatePath(string filePath, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty.", parameterName);
        }
    }

    private static void ValidateText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }
    }
}
