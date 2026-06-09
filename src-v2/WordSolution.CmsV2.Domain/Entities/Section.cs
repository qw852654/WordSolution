using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class Section
{
    private Section()
    {
        Title = string.Empty;
    }

    public Section(
        int teachingTopicId,
        string title,
        string? description = null,
        SectionType type = SectionType.NormalCourse,
        Difficulty difficulty = Difficulty.Unset,
        SectionStatus status = SectionStatus.Draft,
        int sortOrder = 0,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.Positive(teachingTopicId, nameof(TeachingTopicId));
        DomainGuard.NotWhiteSpace(title, nameof(Title));
        DomainGuard.ValidEnum(type, nameof(Type));
        DomainGuard.ValidEnum(difficulty, nameof(Difficulty));
        DomainGuard.ValidEnum(status, nameof(Status));
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));

        TeachingTopicId = teachingTopicId;
        Title = title.Trim();
        Description = description?.Trim();
        Type = type;
        Difficulty = difficulty;
        Status = status;
        SortOrder = sortOrder;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public int TeachingTopicId { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public SectionType Type { get; private set; }

    public Difficulty Difficulty { get; private set; }

    public SectionStatus Status { get; private set; }

    public int SortOrder { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }
}
