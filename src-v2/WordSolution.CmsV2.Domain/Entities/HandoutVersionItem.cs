using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Exceptions;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class HandoutVersionItem
{
    public HandoutVersionItem(
        int handoutVersionId,
        HandoutVersionItemTargetType targetType,
        int targetId,
        int sortOrder,
        string? titleOverride = null,
        string? note = null,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.Positive(handoutVersionId, nameof(HandoutVersionId));
        DomainGuard.ValidEnum(targetType, "HandoutVersionItem.TargetType");
        DomainGuard.Positive(targetId, nameof(TargetId));
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));

        if (targetType is not (HandoutVersionItemTargetType.SectionVariant or HandoutVersionItemTargetType.ContentBlock))
        {
            throw new DomainException("HandoutVersionItem.TargetType only allows SectionVariant or ContentBlock.");
        }

        HandoutVersionId = handoutVersionId;
        TargetType = targetType;
        TargetId = targetId;
        SortOrder = sortOrder;
        TitleOverride = titleOverride?.Trim();
        Note = note?.Trim();
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public int HandoutVersionId { get; private set; }

    public HandoutVersionItemTargetType TargetType { get; private set; }

    public int TargetId { get; private set; }

    public int SortOrder { get; private set; }

    public string? TitleOverride { get; private set; }

    public string? Note { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }
}

