using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Application.SectionVariants;

public sealed record CreateSectionVariantCommand(
    int SectionId,
    string Title,
    string? Description = null,
    SectionVariantType Type = SectionVariantType.Lecture,
    Difficulty Difficulty = Difficulty.Unset,
    SectionVariantStatus Status = SectionVariantStatus.Draft,
    int SortOrder = 0);

public sealed record AddSectionVariantItemCommand(
    int SectionVariantId,
    int SectionItemId,
    int SortOrder,
    string? Note = null);
