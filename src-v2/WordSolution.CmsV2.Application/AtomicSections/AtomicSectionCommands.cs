using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Application.AtomicSections;

public sealed record AddAtomicSectionItemCommand(
    int AtomicSectionId,
    int ContentBlockId,
    ReferenceMode ReferenceMode,
    int? LockedContentBlockVersionId,
    int SortOrder,
    string? TitleOverride = null,
    string? Note = null);

public sealed record RenameAtomicSectionCommand(
    int AtomicSectionId,
    string Title);
