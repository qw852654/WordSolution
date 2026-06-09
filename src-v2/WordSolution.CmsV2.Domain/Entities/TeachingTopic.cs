using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class TeachingTopic
{
    public TeachingTopic(
        string name,
        string? description = null,
        int? parentId = null,
        int sortOrder = 0,
        TeachingTopicStatus status = TeachingTopicStatus.Active,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.NotWhiteSpace(name, nameof(Name));
        DomainGuard.PositiveOrNull(parentId, nameof(ParentId));
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));
        DomainGuard.ValidEnum(status, nameof(Status));

        Name = name.Trim();
        Description = description?.Trim();
        ParentId = parentId;
        SortOrder = sortOrder;
        Status = status;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public int? ParentId { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public int SortOrder { get; private set; }

    public TeachingTopicStatus Status { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }
}

