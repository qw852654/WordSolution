using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class ContentBlockVersionPart
{
    private ContentBlockVersionPart()
    {
        SourceStyleNamesJson = "[]";
    }

    public ContentBlockVersionPart(
        int contentBlockVersionId,
        ContentBlockPartType partType,
        int sortOrder,
        string? plainText = null,
        string? sourceStyleNamesJson = null,
        string? warningMessage = null)
    {
        DomainGuard.Positive(contentBlockVersionId, nameof(ContentBlockVersionId));
        DomainGuard.ValidEnum(partType, nameof(PartType));
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));

        ContentBlockVersionId = contentBlockVersionId;
        PartType = partType;
        SortOrder = sortOrder;
        PlainText = plainText;
        SourceStyleNamesJson = string.IsNullOrWhiteSpace(sourceStyleNamesJson)
            ? "[]"
            : sourceStyleNamesJson.Trim();
        WarningMessage = warningMessage?.Trim();
    }

    public int Id { get; private set; }

    public int ContentBlockVersionId { get; private set; }

    public ContentBlockPartType PartType { get; private set; }

    public int SortOrder { get; private set; }

    public string? PlainText { get; private set; }

    public string SourceStyleNamesJson { get; private set; }

    public string? WarningMessage { get; private set; }
}
