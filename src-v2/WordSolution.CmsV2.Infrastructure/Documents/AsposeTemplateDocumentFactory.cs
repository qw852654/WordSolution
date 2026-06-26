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
        ValidatePath(templateDocxPath, nameof(templateDocxPath));
        if (!File.Exists(templateDocxPath))
        {
            throw new FileNotFoundException(
                "Default ContentBlock DOCX template was not found.",
                templateDocxPath);
        }

        return new Document(templateDocxPath);
    }

    public static void CopyTemplateTo(string templateDocxPath, string outputDocxPath)
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
        File.Copy(templateDocxPath, outputDocxPath, overwrite: true);
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
