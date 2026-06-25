using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class TeachingNoteBinding
{
    private TeachingNoteBinding()
    {
    }

    public TeachingNoteBinding(
        int teachingNoteId,
        TeachingNoteBindingTargetType targetType,
        int targetId,
        DateTimeOffset? createdTime = null)
    {
        DomainGuard.Positive(teachingNoteId, nameof(TeachingNoteId));
        DomainGuard.ValidEnum(targetType, "TeachingNoteBinding.TargetType");
        DomainGuard.Positive(targetId, nameof(TargetId));

        TeachingNoteId = teachingNoteId;
        TargetType = targetType;
        TargetId = targetId;
        CreatedTime = createdTime ?? DateTimeOffset.UtcNow;
    }

    public int Id { get; private set; }

    public int TeachingNoteId { get; private set; }

    public TeachingNoteBindingTargetType TargetType { get; private set; }

    public int TargetId { get; private set; }

    public DateTimeOffset CreatedTime { get; private set; }
}
