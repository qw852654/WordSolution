using Aspose.Words;
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
        ValidateText(handoutTitle, nameof(handoutTitle));
        ValidatePath(templateDocxPath, nameof(templateDocxPath));
        ArgumentNullException.ThrowIfNull(sources);
        ValidatePath(outputDocxPath, nameof(outputDocxPath));

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureParentDirectory(outputDocxPath);

            var outputDocument = new Document(templateDocxPath);
            RemoveHeadersAndFooters(outputDocument);
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

            if (sources.Count == 0)
            {
                builder.Writeln("No content.");
            }

            foreach (var source in sources)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateText(source.Title, nameof(source.Title));
                ValidatePath(source.DocxPath, nameof(source.DocxPath));

                builder.MoveToDocumentEnd();
                builder.InsertBreak(BreakType.ParagraphBreak);
                builder.Font.Name = "Microsoft YaHei";
                builder.Font.Size = 11;
                builder.Font.Bold = true;
                builder.Writeln(source.Title.Trim());
                builder.Font.Bold = false;

                var sourceDocument = new Document(source.DocxPath);
                RemoveHeadersAndFooters(sourceDocument);
                outputDocument.AppendDocument(sourceDocument, ImportFormatMode.KeepSourceFormatting);
            }

            outputDocument.Save(outputDocxPath, SaveFormat.Docx);
        }, cancellationToken);
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
