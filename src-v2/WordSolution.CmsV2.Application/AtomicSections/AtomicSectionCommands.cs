using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Application.AtomicSections;

public sealed record CreateAtomicSectionCommand(
    int SectionId,
    string Title,
    string? Description = null,
    AtomicSectionType Type = AtomicSectionType.Custom,
    Difficulty Difficulty = Difficulty.Unset,
    AtomicSectionStatus Status = AtomicSectionStatus.Draft);

public sealed record AddAtomicSectionItemCommand(
    int AtomicSectionId,
    int ContentBlockId,
    ReferenceMode ReferenceMode,
    int? LockedContentBlockVersionId,
    int? SortOrder = null,
    string? TitleOverride = null,
    string? Note = null,
    int? AtomicSectionPanelId = null,
    AtomicSectionTeachingRole TeachingRole = AtomicSectionTeachingRole.Unclassified,
    int? BeforeAtomicSectionItemId = null,
    int? AfterAtomicSectionItemId = null);

public sealed record AtomicSectionPanelDto(
    int Id,
    int AtomicSectionId,
    string Title,
    AtomicSectionTeachingRole TeachingRole,
    Difficulty Difficulty,
    int SortOrder);

public sealed record CreateAtomicSectionPanelCommand(
    int AtomicSectionId,
    string Title,
    AtomicSectionTeachingRole TeachingRole,
    Difficulty Difficulty = Difficulty.Unset,
    int? BeforeAtomicSectionPanelId = null,
    int? AfterAtomicSectionPanelId = null);

public sealed record UpdateAtomicSectionPanelCommand(
    int AtomicSectionId,
    int AtomicSectionPanelId,
    string Title,
    AtomicSectionTeachingRole TeachingRole,
    Difficulty Difficulty = Difficulty.Unset);

public enum AtomicSectionPanelMoveDirection
{
    Up,
    Down
}

public sealed record MoveAtomicSectionPanelCommand(
    int AtomicSectionId,
    int AtomicSectionPanelId,
    AtomicSectionPanelMoveDirection Direction);

public sealed record DeleteAtomicSectionPanelCommand(
    int AtomicSectionId,
    int AtomicSectionPanelId);

public sealed record DeleteAtomicSectionPanelResult(
    int AtomicSectionId,
    int AtomicSectionPanelId,
    int RemovedAtomicSectionItemCount);

public sealed record ChangeAtomicSectionItemClassificationCommand(
    int AtomicSectionId,
    int AtomicSectionItemId,
    AtomicSectionTeachingRole TeachingRole,
    Difficulty Difficulty);

public enum AtomicSectionItemMoveDirection
{
    Up,
    Down
}

public sealed record MoveAtomicSectionItemCommand(
    int AtomicSectionId,
    int AtomicSectionItemId,
    AtomicSectionItemMoveDirection Direction);

public sealed record RemoveAtomicSectionItemCommand(
    int AtomicSectionId,
    int AtomicSectionItemId);

public sealed record RenameAtomicSectionCommand(
    int AtomicSectionId,
    string Title);
