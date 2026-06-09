using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class HandoutVersion
{
    public HandoutVersion(
        int handoutId,
        string title,
        string? description = null,
        HandoutVersionType type = HandoutVersionType.Normal,
        HandoutVersionStatus status = HandoutVersionStatus.Draft,
        int sortOrder = 0,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.Positive(handoutId, nameof(HandoutId));
        DomainGuard.NotWhiteSpace(title, nameof(Title));
        DomainGuard.ValidEnum(type, nameof(Type));
        DomainGuard.ValidEnum(status, nameof(Status));
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));

        HandoutId = handoutId;
        Title = title.Trim();
        Description = description?.Trim();
        Type = type;
        Status = status;
        SortOrder = sortOrder;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public int HandoutId { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public HandoutVersionType Type { get; private set; }

    public HandoutVersionStatus Status { get; private set; }

    public int SortOrder { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }
}

