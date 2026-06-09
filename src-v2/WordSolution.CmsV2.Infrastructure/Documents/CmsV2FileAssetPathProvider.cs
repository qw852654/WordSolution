using WordSolution.CmsV2.Domain.Documents;
using System.Globalization;

namespace WordSolution.CmsV2.Infrastructure.Documents;

public sealed class CmsV2FileAssetPathProvider : ICmsV2FileAssetPathProvider
{
    public string GetContentBlockDocxPath(
        string bankRootDirectory,
        int contentBlockId,
        int versionNumber)
    {
        return GetContentBlockVersionPath(
            bankRootDirectory,
            "source",
            contentBlockId,
            versionNumber,
            ".docx");
    }

    public string GetContentBlockHtmlPreviewPath(
        string bankRootDirectory,
        int contentBlockId,
        int versionNumber)
    {
        return GetContentBlockVersionPath(
            bankRootDirectory,
            "html",
            contentBlockId,
            versionNumber,
            ".html");
    }

    public string GetContentBlockPlainTextPath(
        string bankRootDirectory,
        int contentBlockId,
        int versionNumber)
    {
        return GetContentBlockVersionPath(
            bankRootDirectory,
            "text",
            contentBlockId,
            versionNumber,
            ".txt");
    }

    public string GetGeneratedHandoutDocxPath(
        string bankRootDirectory,
        int handoutVersionId,
        int outputFormId,
        string outputFormTitle,
        DateTimeOffset generatedTime)
    {
        ValidateBankRootDirectory(bankRootDirectory);
        ValidatePositive(handoutVersionId, nameof(handoutVersionId));
        ValidatePositive(outputFormId, nameof(outputFormId));

        var rootDirectory = Path.GetFullPath(bankRootDirectory.Trim());
        var timestamp = generatedTime.UtcDateTime.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
        var safeTitle = SanitizeFileName(outputFormTitle);

        return Path.Combine(
            rootDirectory,
            "handouts",
            "generated",
            handoutVersionId.ToString(),
            $"{timestamp}-{outputFormId}-{safeTitle}.docx");
    }

    private static string GetContentBlockVersionPath(
        string bankRootDirectory,
        string assetKind,
        int contentBlockId,
        int versionNumber,
        string extension)
    {
        ValidateBankRootDirectory(bankRootDirectory);
        ValidatePositive(contentBlockId, nameof(contentBlockId));
        ValidatePositive(versionNumber, nameof(versionNumber));

        var rootDirectory = Path.GetFullPath(bankRootDirectory.Trim());

        return Path.Combine(
            rootDirectory,
            "content-blocks",
            assetKind,
            contentBlockId.ToString(),
            $"v{versionNumber}{extension}");
    }

    private static void ValidateBankRootDirectory(string bankRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(bankRootDirectory))
        {
            throw new ArgumentException("Bank root directory cannot be empty.", nameof(bankRootDirectory));
        }
    }

    private static void ValidatePositive(int value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be greater than zero.");
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        var candidate = string.IsNullOrWhiteSpace(fileName) ? "handout" : fileName.Trim();
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            candidate = candidate.Replace(invalidChar, '-');
        }

        candidate = candidate.Trim('.', ' ');

        return candidate.Length == 0 ? "handout" : candidate;
    }
}
