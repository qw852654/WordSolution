using Aspose.Words;
using WordSolution.CmsV2.Domain.Documents;

namespace WordSolution.CmsV2.Infrastructure.Documents;

internal static class AsposeTemplateDocumentFactory
{
    public static string ResolveDefaultTemplateDocxPath()
    {
        return new OutputTemplatePathResolver()
            .ResolveTemplateDocxPath(OutputTemplatePaths.RuntimeDefaultTemplateDocxPath);
    }

    public static Document CreateDocumentCopy(string templateDocxPath)
    {
        return CreateDocumentCopy(templateDocxPath, TemplateHeaderFooterMode.Remove);
    }

    public static Document CreateDocumentCopy(
        string templateDocxPath,
        TemplateHeaderFooterMode headerFooterMode)
    {
        ValidatePath(templateDocxPath, nameof(templateDocxPath));
        if (!File.Exists(templateDocxPath))
        {
            throw new FileNotFoundException(
                "Default ContentBlock DOCX template was not found.",
                templateDocxPath);
        }

        var document = new Document(templateDocxPath);
        ApplyHeaderFooterMode(document, headerFooterMode);

        return document;
    }

    public static void CopyTemplateTo(string templateDocxPath, string outputDocxPath)
    {
        CopyTemplateTo(templateDocxPath, outputDocxPath, TemplateHeaderFooterMode.Remove);
    }

    public static void CopyTemplateTo(
        string templateDocxPath,
        string outputDocxPath,
        TemplateHeaderFooterMode headerFooterMode)
    {
        ValidatePath(templateDocxPath, nameof(templateDocxPath));
        ValidatePath(outputDocxPath, nameof(outputDocxPath));
        if (!File.Exists(templateDocxPath))
        {
            throw new FileNotFoundException(
                "Default ContentBlock DOCX template was not found.",
                templateDocxPath);
        }

        EnsureParentDirectory(outputDocxPath);
        var document = CreateDocumentCopy(templateDocxPath, headerFooterMode);
        document.Save(outputDocxPath, SaveFormat.Docx);
    }

    private static void ApplyHeaderFooterMode(Document document, TemplateHeaderFooterMode mode)
    {
        if (mode == TemplateHeaderFooterMode.Preserve)
        {
            return;
        }

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

internal enum TemplateHeaderFooterMode
{
    Remove,
    Preserve
}
