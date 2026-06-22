using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Application.SectionVariants;

public sealed record CreateSectionVariantCommand(
    int SectionId,
    string Title,
    string? Description = null,
    SectionVariantType Type = SectionVariantType.Lecture,
    Difficulty Difficulty = Difficulty.Unset,
    IReadOnlyList<int>? SelectedSectionItemIds = null);

public sealed record AddSectionVariantItemCommand(
    int SectionVariantId,
    int SectionItemId,
    int SortOrder,
    string? Note = null);

public sealed record PreviewSectionVariantSelectionCommand(
    int SectionId,
    Difficulty Difficulty);

public sealed record SectionVariantSelectionCandidateDto(
    int SectionItemId,
    int? ParentItemId,
    int SourceSortOrder,
    SectionItemTargetType TargetType,
    int TargetId,
    Difficulty ResolvedDifficulty,
    bool DefaultSelected,
    bool Selectable,
    string? UnavailableReason);
