namespace WordSolution.CmsV2.Domain.Documents;

public static class OutputTemplatePaths
{
    public const string RuntimeDefaultTemplateDocxPath =
        "Documents/Templates/content-block-default.docx";

    public const string LegacyDefaultTemplateDocxPath =
        "src-v2/WordSolution.CmsV2.Infrastructure/Documents/Templates/content-block-default.docx";

    public static bool IsDefaultTemplatePath(string templateDocxPath)
    {
        var normalizedPath = NormalizeTemplatePath(templateDocxPath);
        return string.Equals(
                normalizedPath,
                NormalizeTemplatePath(RuntimeDefaultTemplateDocxPath),
                StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                normalizedPath,
                NormalizeTemplatePath(LegacyDefaultTemplateDocxPath),
                StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeTemplatePath(string templateDocxPath)
    {
        return templateDocxPath.Trim().Replace('\\', '/');
    }
}
