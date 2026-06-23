using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Exceptions;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class AtomicSectionPanel
{
    private AtomicSectionPanel()
    {
        Title = string.Empty;
    }

    public AtomicSectionPanel(
        int atomicSectionId,
        string title,
        AtomicSectionTeachingRole teachingRole,
        Difficulty difficulty = Difficulty.Unset,
        int sortOrder = 0,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.Positive(atomicSectionId, nameof(AtomicSectionId));
        DomainGuard.NotWhiteSpace(title, nameof(Title));
        DomainGuard.ValidEnum(teachingRole, nameof(TeachingRole));
        DomainGuard.ValidEnum(difficulty, nameof(Difficulty));
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));

        if (teachingRole == AtomicSectionTeachingRole.Unclassified)
        {
            throw new DomainException("AtomicSectionPanel.TeachingRole cannot be Unclassified.");
        }

        AtomicSectionId = atomicSectionId;
        Title = title.Trim();
        TeachingRole = teachingRole;
        Difficulty = difficulty;
        SortOrder = sortOrder;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public int AtomicSectionId { get; private set; }

    public string Title { get; private set; }

    public AtomicSectionTeachingRole TeachingRole { get; private set; }

    public Difficulty Difficulty { get; private set; }

    public int SortOrder { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }

    public void Rename(string title, DateTimeOffset? updatedTime = null)
    {
        DomainGuard.NotWhiteSpace(title, nameof(Title));
        Title = title.Trim();
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public void ChangeClassification(
        AtomicSectionTeachingRole teachingRole,
        Difficulty difficulty,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.ValidEnum(teachingRole, nameof(TeachingRole));
        DomainGuard.ValidEnum(difficulty, nameof(Difficulty));

        if (teachingRole == AtomicSectionTeachingRole.Unclassified)
        {
            throw new DomainException("AtomicSectionPanel.TeachingRole cannot be Unclassified.");
        }

        TeachingRole = teachingRole;
        Difficulty = difficulty;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public void ChangeSortOrder(int sortOrder, DateTimeOffset? updatedTime = null)
    {
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));
        SortOrder = sortOrder;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }
}
