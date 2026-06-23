using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class AtomicSectionItem
{
    private AtomicSectionItem()
    {
    }

    public AtomicSectionItem(
        int atomicSectionId,
        int contentBlockId,
        ReferenceMode referenceMode,
        int? lockedContentBlockVersionId,
        int sortOrder,
        string? titleOverride = null,
        string? note = null,
        DateTimeOffset? updatedTime = null,
        int? atomicSectionPanelId = null,
        AtomicSectionTeachingRole teachingRole = AtomicSectionTeachingRole.Unclassified)
    {
        DomainGuard.Positive(atomicSectionId, nameof(AtomicSectionId));
        DomainGuard.Positive(contentBlockId, nameof(ContentBlockId));
        DomainGuard.LockedVersionRequiresId(referenceMode, lockedContentBlockVersionId);
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));
        DomainGuard.PositiveOrNull(atomicSectionPanelId, nameof(AtomicSectionPanelId));
        DomainGuard.ValidEnum(teachingRole, nameof(TeachingRole));

        AtomicSectionId = atomicSectionId;
        ContentBlockId = contentBlockId;
        AtomicSectionPanelId = atomicSectionPanelId;
        TeachingRole = teachingRole;
        ReferenceMode = referenceMode;
        LockedContentBlockVersionId = lockedContentBlockVersionId;
        TitleOverride = titleOverride?.Trim();
        SortOrder = sortOrder;
        Note = note?.Trim();
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public int AtomicSectionId { get; private set; }

    public int ContentBlockId { get; private set; }

    public int? AtomicSectionPanelId { get; private set; }

    public AtomicSectionTeachingRole TeachingRole { get; private set; }

    public ReferenceMode ReferenceMode { get; private set; }

    public int? LockedContentBlockVersionId { get; private set; }

    public string? TitleOverride { get; private set; }

    public int SortOrder { get; private set; }

    public string? Note { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }

    public void ChangeSortOrder(int sortOrder, DateTimeOffset? updatedTime = null)
    {
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));
        SortOrder = sortOrder;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public void ChangeClassification(
        int? atomicSectionPanelId,
        AtomicSectionTeachingRole teachingRole,
        int sortOrder,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.PositiveOrNull(atomicSectionPanelId, nameof(AtomicSectionPanelId));
        DomainGuard.ValidEnum(teachingRole, nameof(TeachingRole));
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));

        AtomicSectionPanelId = atomicSectionPanelId;
        TeachingRole = teachingRole;
        SortOrder = sortOrder;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }
}
