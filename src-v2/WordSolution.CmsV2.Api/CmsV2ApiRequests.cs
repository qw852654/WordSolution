using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Api;

public sealed record CreateTeachingTopicRequest(
    string Name,
    string? Description = null,
    int? ParentId = null,
    int SortOrder = 0,
    TeachingTopicStatus Status = TeachingTopicStatus.Active);

public sealed record CreateSectionRequest(
    int TeachingTopicId,
    string Title,
    string? Description = null,
    SectionType Type = SectionType.NormalCourse,
    Difficulty Difficulty = Difficulty.Unset,
    SectionStatus Status = SectionStatus.Draft,
    int SortOrder = 0);

public sealed record CreateAtomicSectionRequest(
    string Title,
    string? Description = null,
    AtomicSectionType Type = AtomicSectionType.Custom,
    AtomicSectionStatus Status = AtomicSectionStatus.Draft);

public sealed record CreateContentBlockWithBlankDocumentRequest(
    string Title,
    ContentBlockType BlockType,
    string? Summary = null,
    Difficulty Difficulty = Difficulty.Unset,
    QuestionType? QuestionType = null,
    ContentBlockStatus Status = ContentBlockStatus.Draft);

public sealed record SetCurrentContentBlockVersionRequest(int ContentBlockVersionId);

public sealed record AddContentBlockRelationRequest(
    int ChildBlockId,
    ReferenceMode ReferenceMode,
    int? LockedContentBlockVersionId,
    int SortOrder,
    string? TitleOverride = null,
    string? Note = null);

public sealed record AddSectionItemRequest(
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

public sealed record AddAtomicSectionItemRequest(
    int ContentBlockId,
    ReferenceMode ReferenceMode,
    int? LockedContentBlockVersionId,
    int SortOrder,
    string? TitleOverride = null,
    string? Note = null);

public sealed record CreateSectionVariantRequest(
    int SectionId,
    string Title,
    string? Description = null,
    SectionVariantType Type = SectionVariantType.Lecture,
    Difficulty Difficulty = Difficulty.Unset,
    SectionVariantStatus Status = SectionVariantStatus.Draft,
    int SortOrder = 0);

public sealed record AddSectionVariantItemRequest(
    int SectionItemId,
    int SortOrder,
    string? Note = null);

public sealed record CreateHandoutRequest(
    string Title,
    string? Description = null,
    HandoutStatus Status = HandoutStatus.Draft);

public sealed record CreateHandoutVersionRequest(
    string Title,
    string? Description = null,
    HandoutVersionType Type = HandoutVersionType.Normal,
    HandoutVersionStatus Status = HandoutVersionStatus.Draft,
    int SortOrder = 0);

public sealed record AddHandoutVersionItemRequest(
    HandoutVersionItemTargetType TargetType,
    int TargetId,
    int SortOrder,
    string? TitleOverride = null,
    string? Note = null);

public sealed record CreateOutputTemplateRequest(
    string Title,
    string TemplateDocxPath,
    string? Description = null,
    OutputTemplateStatus Status = OutputTemplateStatus.Active);

public sealed record CreateOutputFormRequest(
    int HandoutVersionId,
    int OutputTemplateId,
    string Title,
    OutputAudience Audience,
    OutputFormat OutputFormat,
    VisibilityMode VisibilityMode,
    OutputFormStatus Status = OutputFormStatus.Active,
    int SortOrder = 0);

public sealed record GenerateHandoutWordRequest(DateTimeOffset? GeneratedTime = null);

public sealed record CreateTeachingNoteRequest(
    TeachingNoteTargetType TargetType,
    int TargetId,
    TeachingNoteType NoteType,
    string Title,
    string Content,
    TeachingNoteStatus Status = TeachingNoteStatus.Active);
