using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class AtomicSection
{
    private AtomicSection()
    {
        Title = string.Empty;
    }

    public AtomicSection(
        string title,
        string? description = null,
        AtomicSectionType type = AtomicSectionType.Custom,
        AtomicSectionStatus status = AtomicSectionStatus.Draft,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.NotWhiteSpace(title, nameof(Title));
        DomainGuard.ValidEnum(type, nameof(Type));
        DomainGuard.ValidEnum(status, nameof(Status));

        Title = title.Trim();
        Description = description?.Trim();
        Type = type;
        Status = status;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public AtomicSectionType Type { get; private set; }

    public AtomicSectionStatus Status { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }
}
