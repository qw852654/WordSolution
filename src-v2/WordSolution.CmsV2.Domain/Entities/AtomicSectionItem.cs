using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class AtomicSectionItem
{
    public AtomicSectionItem(
        int atomicSectionId,
        int contentBlockId,
        ReferenceMode referenceMode,
        int? lockedContentBlockVersionId,
        int sortOrder,
        string? titleOverride = null,
        string? note = null,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.Positive(atomicSectionId, nameof(AtomicSectionId));
        DomainGuard.Positive(contentBlockId, nameof(ContentBlockId));
        DomainGuard.LockedVersionRequiresId(referenceMode, lockedContentBlockVersionId);
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));

        AtomicSectionId = atomicSectionId;
        ContentBlockId = contentBlockId;
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

    public ReferenceMode ReferenceMode { get; private set; }

    public int? LockedContentBlockVersionId { get; private set; }

    public string? TitleOverride { get; private set; }

    public int SortOrder { get; private set; }

    public string? Note { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }
}

