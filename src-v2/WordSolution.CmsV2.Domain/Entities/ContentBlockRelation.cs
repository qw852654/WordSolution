using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Exceptions;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class ContentBlockRelation
{
    private ContentBlockRelation()
    {
    }

    public ContentBlockRelation(
        int parentBlockId,
        int childBlockId,
        ReferenceMode referenceMode,
        int? lockedContentBlockVersionId,
        int sortOrder,
        string? titleOverride = null,
        string? note = null,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.Positive(parentBlockId, nameof(ParentBlockId));
        DomainGuard.Positive(childBlockId, nameof(ChildBlockId));
        DomainGuard.LockedVersionRequiresId(referenceMode, lockedContentBlockVersionId);
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));

        if (parentBlockId == childBlockId)
        {
            throw new DomainException("ContentBlockRelation cannot directly include itself.");
        }

        ParentBlockId = parentBlockId;
        ChildBlockId = childBlockId;
        ReferenceMode = referenceMode;
        LockedContentBlockVersionId = lockedContentBlockVersionId;
        TitleOverride = titleOverride?.Trim();
        SortOrder = sortOrder;
        Note = note?.Trim();
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public int ParentBlockId { get; private set; }

    public int ChildBlockId { get; private set; }

    public ReferenceMode ReferenceMode { get; private set; }

    public int? LockedContentBlockVersionId { get; private set; }

    public string? TitleOverride { get; private set; }

    public int SortOrder { get; private set; }

    public string? Note { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }
}
