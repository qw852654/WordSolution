using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class TeachingTopic
{
    private TeachingTopic()
    {
        Name = string.Empty;
    }

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

    public void Rename(string name, string? description = null, DateTimeOffset? updatedTime = null)
    {
        DomainGuard.NotWhiteSpace(name, nameof(Name));

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public void MoveTo(int? parentId, int sortOrder, DateTimeOffset? updatedTime = null)
    {
        DomainGuard.PositiveOrNull(parentId, nameof(ParentId));
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));

        ParentId = parentId;
        SortOrder = sortOrder;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public void SetSortOrder(int sortOrder, DateTimeOffset? updatedTime = null)
    {
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));

        SortOrder = sortOrder;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }
}
