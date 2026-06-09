using Aspose.Words;
using Aspose.Words.Saving;
using WordSolution.CmsV2.Domain.Documents;

namespace WordSolution.CmsV2.Infrastructure.Documents;

public sealed class AsposeContentBlockDocumentProcessor : IContentBlockDocumentProcessor
{
    public Task CreateBlankDocxAsync(
        string docxPath,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(docxPath, nameof(docxPath));

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureParentDirectory(docxPath);

            var document = new Document();
            document.Save(docxPath, SaveFormat.Docx);
        }, cancellationToken);
    }

    public Task GenerateHtmlPreviewAsync(
        string docxPath,
        string htmlPreviewPath,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(docxPath, nameof(docxPath));
        ValidatePath(htmlPreviewPath, nameof(htmlPreviewPath));

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureParentDirectory(htmlPreviewPath);

            var document = new Document(docxPath);
            RemoveHeadersAndFooters(document);
            var saveOptions = new HtmlSaveOptions
            {
                ExportImagesAsBase64 = true,
                PrettyFormat = true
            };

            document.Save(htmlPreviewPath, saveOptions);
        }, cancellationToken);
    }

    public Task<string> ExtractPlainTextAsync(
        string docxPath,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(docxPath, nameof(docxPath));

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var document = new Document(docxPath);
            RemoveHeadersAndFooters(document);

            return document.ToString(SaveFormat.Text).Trim();
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
}
