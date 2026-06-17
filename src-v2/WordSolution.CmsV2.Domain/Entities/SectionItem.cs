using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Exceptions;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class SectionItem
{
    private SectionItem()
    {
    }

    public SectionItem(
        int sectionId,
        SectionItemTargetType targetType,
        int targetId,
        ReferenceMode referenceMode,
        int? lockedContentBlockVersionId,
        int sortOrder,
        string? titleOverride = null,
        int? parentItemId = null,
        SelectionLayer? selectionLayer = null,
        TeachingUse? teachingUseOverride = null,
        SectionStatus status = SectionStatus.Active,
        string? note = null,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.Positive(sectionId, nameof(SectionId));
        DomainGuard.ValidEnum(targetType, "SectionItem.TargetType");
        DomainGuard.Positive(targetId, nameof(TargetId));
        DomainGuard.PositiveOrNull(parentItemId, nameof(ParentItemId));
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));
        DomainGuard.ValidEnum(status, nameof(Status));

        if (selectionLayer.HasValue)
        {
            DomainGuard.ValidEnum(selectionLayer.Value, nameof(SelectionLayer));
        }

        if (teachingUseOverride.HasValue)
        {
            DomainGuard.ValidEnum(teachingUseOverride.Value, nameof(TeachingUseOverride));
        }

        if (targetType == SectionItemTargetType.ContentBlock)
        {
            DomainGuard.LockedVersionRequiresId(referenceMode, lockedContentBlockVersionId);
        }
        else if (referenceMode != ReferenceMode.FollowLatest || lockedContentBlockVersionId.HasValue)
        {
            throw new DomainException("SectionItem.ReferenceMode only applies when TargetType is ContentBlock.");
        }

        SectionId = sectionId;
        TargetType = targetType;
        TargetId = targetId;
        ReferenceMode = referenceMode;
        LockedContentBlockVersionId = lockedContentBlockVersionId;
        TitleOverride = titleOverride?.Trim();
        ParentItemId = parentItemId;
        SortOrder = sortOrder;
        SelectionLayer = selectionLayer;
        TeachingUseOverride = teachingUseOverride;
        Status = status;
        Note = note?.Trim();
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public int SectionId { get; private set; }

    public SectionItemTargetType TargetType { get; private set; }

    public int TargetId { get; private set; }

    public ReferenceMode ReferenceMode { get; private set; }

    public int? LockedContentBlockVersionId { get; private set; }

    public string? TitleOverride { get; private set; }

    public int? ParentItemId { get; private set; }

    public int SortOrder { get; private set; }

    public SelectionLayer? SelectionLayer { get; private set; }

    public TeachingUse? TeachingUseOverride { get; private set; }

    public SectionStatus Status { get; private set; }

    public string? Note { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }

    public void ChangeSortOrder(int sortOrder, DateTimeOffset? updatedTime = null)
    {
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));

        SortOrder = sortOrder;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }
}
