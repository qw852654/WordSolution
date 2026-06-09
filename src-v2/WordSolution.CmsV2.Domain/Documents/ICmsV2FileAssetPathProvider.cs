namespace WordSolution.CmsV2.Domain.Documents;

public interface ICmsV2FileAssetPathProvider
{
    string GetContentBlockDocxPath(
        string bankRootDirectory,
        int contentBlockId,
        int versionNumber);

    string GetContentBlockHtmlPreviewPath(
        string bankRootDirectory,
        int contentBlockId,
        int versionNumber);

    string GetContentBlockPlainTextPath(
        string bankRootDirectory,
        int contentBlockId,
        int versionNumber);

    string GetGeneratedHandoutDocxPath(
        string bankRootDirectory,
        int handoutVersionId,
        int outputFormId,
        string outputFormTitle,
        DateTimeOffset generatedTime);
}
