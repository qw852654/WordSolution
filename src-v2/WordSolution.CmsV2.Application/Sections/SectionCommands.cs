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
