using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class ContentBlockVersion
{
    private ContentBlockVersion()
    {
        DocxPath = string.Empty;
    }

    public ContentBlockVersion(
        int contentBlockId,
        int versionNumber,
        string docxPath,
        string? htmlPreviewPath = null,
        string? plainText = null,
        bool isCurrent = false,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.Positive(contentBlockId, nameof(ContentBlockId));
        DomainGuard.Positive(versionNumber, nameof(VersionNumber));
        DomainGuard.NotWhiteSpace(docxPath, nameof(DocxPath));

        ContentBlockId = contentBlockId;
        VersionNumber = versionNumber;
        DocxPath = docxPath.Trim();
        HtmlPreviewPath = htmlPreviewPath?.Trim();
        PlainText = plainText;
        IsCurrent = isCurrent;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public int ContentBlockId { get; private set; }

    public int VersionNumber { get; private set; }

    public string DocxPath { get; private set; }

    public string? HtmlPreviewPath { get; private set; }

    public string? PlainText { get; private set; }

    public bool IsCurrent { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }

    public void MarkCurrent(DateTimeOffset? updatedTime = null)
    {
        IsCurrent = true;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public void MarkNotCurrent(DateTimeOffset? updatedTime = null)
    {
        IsCurrent = false;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }
}
