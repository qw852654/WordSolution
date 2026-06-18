using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Application.Sections;

public sealed record AddSectionItemCommand(
    int SectionId,
    SectionItemTargetType TargetType,
    int TargetId,
    ReferenceMode ReferenceMode,
    int? LockedContentBlockVersionId,
    int SortOrder,
    string? TitleOverride = null,
    int? ParentItemId = null,
    SelectionLayer? SelectionLayer = null,
    TeachingUse? TeachingUseOverride = null,
    SectionStatus Status = SectionStatus.Active,
    string? Note = null);

public enum SectionItemMoveDirection
{
    Up,
    Down
}

public sealed record MoveSectionItemCommand(
    int SectionId,
    int SectionItemId,
    SectionItemMoveDirection Direction);

public sealed record RemoveSectionItemCommand(
    int SectionId,
    int SectionItemId);

public sealed record WrapSectionItemsAsAtomicSectionCommand(
    int SectionId,
    IReadOnlyList<int> SectionItemIds,
    string Title,
    string? Description = null,
    AtomicSectionType Type = AtomicSectionType.Custom,
    Difficulty Difficulty = Difficulty.Unset,
    AtomicSectionStatus Status = AtomicSectionStatus.Draft);

public sealed record WrapSectionItemsAsAtomicSectionResult(
    int SectionId,
    int AtomicSectionId,
    int SectionItemId,
    IReadOnlyList<int> WrappedSectionItemIds,
    IReadOnlyList<int> AtomicSectionItemIds);
