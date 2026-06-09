using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class SectionVariantItem
{
    private SectionVariantItem()
    {
    }

    public SectionVariantItem(
        int sectionVariantId,
        int sectionItemId,
        int sortOrder,
        string? note = null,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.Positive(sectionVariantId, nameof(SectionVariantId));
        DomainGuard.Positive(sectionItemId, nameof(SectionItemId));
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));

        SectionVariantId = sectionVariantId;
        SectionItemId = sectionItemId;
        SortOrder = sortOrder;
        Note = note?.Trim();
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public int SectionVariantId { get; private set; }

    public int SectionItemId { get; private set; }

    public int SortOrder { get; private set; }

    public string? Note { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }
}
