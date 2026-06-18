using Aspose.Words;
using Aspose.Words.Saving;
using WordSolution.CmsV2.Domain.Documents;

namespace WordSolution.CmsV2.Infrastructure.Documents;

public sealed class AsposeContentBlockDocumentProcessor : IContentBlockDocumentProcessor
{
    private static readonly string DefaultTemplateDocxPath = Path.Combine(
        AppContext.BaseDirectory,
        "Documents",
        "Templates",
        "content-block-default.docx");

    private readonly string _templateDocxPath;

    public AsposeContentBlockDocumentProcessor()
        : this(DefaultTemplateDocxPath)
    {
    }

    public AsposeContentBlockDocumentProcessor(string templateDocxPath)
    {
        ValidatePath(templateDocxPath, nameof(templateDocxPath));
        _templateDocxPath = Path.GetFullPath(templateDocxPath);
    }

    public Task CreateBlankDocxAsync(
        string docxPath,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(docxPath, nameof(docxPath));

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureParentDirectory(docxPath);

            if (!File.Exists(_templateDocxPath))
            {
                throw new FileNotFoundException(
                    "Default ContentBlock DOCX template was not found.",
                    _templateDocxPath);
            }

            File.Copy(_templateDocxPath, docxPath, overwrite: true);
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
