using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class TagBinding
{
    private TagBinding()
    {
    }

    public TagBinding(
        int tagId,
        TagBindingTargetType targetType,
        int targetId,
        DateTimeOffset? createdTime = null,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.Positive(tagId, nameof(TagId));
        DomainGuard.ValidEnum(targetType, "TagBinding.TargetType");
        DomainGuard.Positive(targetId, nameof(TargetId));

        TagId = tagId;
        TargetType = targetType;
        TargetId = targetId;
        CreatedTime = createdTime ?? DateTimeOffset.UtcNow;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime ?? CreatedTime);
    }

    public int Id { get; private set; }

    public int TagId { get; private set; }

    public TagBindingTargetType TargetType { get; private set; }

    public int TargetId { get; private set; }

    public DateTimeOffset CreatedTime { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }
}
