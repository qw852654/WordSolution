using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class SectionVariant
{
    public SectionVariant(
        int sectionId,
        string title,
        string? description = null,
        SectionVariantType type = SectionVariantType.Lecture,
        Difficulty difficulty = Difficulty.Unset,
        SectionVariantStatus status = SectionVariantStatus.Draft,
        int sortOrder = 0,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.Positive(sectionId, nameof(SectionId));
        DomainGuard.NotWhiteSpace(title, nameof(Title));
        DomainGuard.ValidEnum(type, nameof(Type));
        DomainGuard.ValidEnum(difficulty, nameof(Difficulty));
        DomainGuard.ValidEnum(status, nameof(Status));
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));

        SectionId = sectionId;
        Title = title.Trim();
        Description = description?.Trim();
        Type = type;
        Difficulty = difficulty;
        Status = status;
        SortOrder = sortOrder;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public int SectionId { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public SectionVariantType Type { get; private set; }

    public Difficulty Difficulty { get; private set; }

    public SectionVariantStatus Status { get; private set; }

    public int SortOrder { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }
}

