using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class Handout
{
    private Handout()
    {
        Title = string.Empty;
    }

    public Handout(
        string title,
        string? description = null,
        HandoutStatus status = HandoutStatus.Draft,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.NotWhiteSpace(title, nameof(Title));
        DomainGuard.ValidEnum(status, nameof(Status));

        Title = title.Trim();
        Description = description?.Trim();
        Status = status;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public HandoutStatus Status { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }

    public void UpdateDetails(
        string title,
        string? description,
        HandoutStatus status,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.NotWhiteSpace(title, nameof(Title));
        DomainGuard.ValidEnum(status, nameof(Status));

        Title = title.Trim();
        Description = description?.Trim();
        Status = status;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }
}
