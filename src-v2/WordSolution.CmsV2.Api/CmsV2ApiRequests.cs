using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Application.ContentBlocks;

namespace WordSolution.CmsV2.Api;

public sealed record CreateTeachingTopicRequest(
    string Name,
    string? Description = null,
    int? ParentId = null,
    int SortOrder = 0,
    TeachingTopicStatus Status = TeachingTopicStatus.Active);

public sealed record CreateTeachingTopicChildRequest(
    string Name,
    string? Description = null,
    TeachingTopicStatus Status = TeachingTopicStatus.Active);

public sealed record CreateTeachingTopicNextSiblingRequest(
    string Name,
    string? Description = null,
    TeachingTopicStatus Status = TeachingTopicStatus.Active);

public sealed record RenameTeachingTopicRequest(
    string Name,
    string? Description = null);

public sealed record CreateSectionForTeachingTopicRequest(
    string? Title = null,
    string? Description = null,
    SectionType Type = SectionType.NormalCourse,
    Difficulty Difficulty = Difficulty.Unset,
    SectionStatus Status = SectionStatus.Draft);

public sealed record CreateSectionRequest(
    int TeachingTopicId,
    string Title,
    string? Description = null,
    SectionType Type = SectionType.NormalCourse,
    Difficulty Difficulty = Difficulty.Unset,
    SectionStatus Status = SectionStatus.Draft,
    int SortOrder = 0);

public sealed record CreateAtomicSectionRequest(
    int SectionId,
    string Title,
    string? Description = null,
    AtomicSectionType Type = AtomicSectionType.Custom,
    Difficulty Difficulty = Difficulty.Unset,
    AtomicSectionStatus Status = AtomicSectionStatus.Draft);

public sealed record CreateContentBlockWithBlankDocumentRequest(
    int SectionId,
    string Title,
    ContentBlockType BlockType,
    string? Summary = null,
    Difficulty Difficulty = Difficulty.Unset,
    QuestionType? QuestionType = null,
    ContentBlockStatus Status = ContentBlockStatus.Draft);

public sealed record CreateContentBlockRequest(
    int SectionId,
    string Title,
    ContentBlockType BlockType,
    string? Summary = null,
    Difficulty Difficulty = Difficulty.Unset,
    QuestionType? QuestionType = null,
    ContentBlockStatus Status = ContentBlockStatus.Draft);

public sealed record CreateQuestionImportSessionRequest(
    InsertQuestionContext Context,
    bool OpenWord = true);

public sealed record ConfirmQuestionImportRequest(
    IReadOnlyList<ConfirmQuestionImportCandidateSelection> Candidates);

public sealed record SetCurrentContentBlockVersionRequest(int ContentBlockVersionId);

public sealed record ChangeContentBlockDifficultyRequest(Difficulty Difficulty);

public sealed record CreateContentBlockEditSessionRequest(bool OpenWord = true);

public sealed record ContentBlockEditSessionResponse(
    string SessionId,
    int ContentBlockId,
    int SourceContentBlockVersionId,
    string Status,
    string LaunchMode,
    bool OpenedByServer,
    string? Message,
    DateTimeOffset CreatedTime,
    DateTimeOffset UpdatedTime);

public sealed record SyncContentBlockEditSessionResponse(
    string SessionId,
    int ContentBlockId,
    bool Changed,
    int? NewContentBlockVersionId,
    int? CurrentVersionNumber,
    string Status,
    string? Message);

public sealed record AddContentBlockRelationRequest(
    int ChildBlockId,
    ReferenceMode ReferenceMode,
    int? LockedContentBlockVersionId,
    int SortOrder,
    string? TitleOverride = null,
    string? Note = null);

public sealed record MoveContentBlockRelationRequest(string Direction);

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

public sealed record WrapSectionItemsAsAtomicSectionRequest(
    int[] SectionItemIds,
    string Title,
    string? Description = null,
    AtomicSectionType Type = AtomicSectionType.Custom,
    Difficulty Difficulty = Difficulty.Unset,
    AtomicSectionStatus Status = AtomicSectionStatus.Draft);

public sealed record MoveSectionItemRequest(string Direction);

public sealed record AddAtomicSectionItemRequest(
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

public sealed record MoveAtomicSectionItemRequest(string Direction);

public sealed record RenameAtomicSectionRequest(string Title);

public sealed record ChangeAtomicSectionDifficultyRequest(Difficulty Difficulty);

public sealed record CreateAtomicSectionPanelRequest(
    string Title,
    AtomicSectionTeachingRole TeachingRole,
    Difficulty Difficulty = Difficulty.Unset,
    int? BeforeAtomicSectionPanelId = null,
    int? AfterAtomicSectionPanelId = null);

public sealed record UpdateAtomicSectionPanelRequest(
    string Title,
    AtomicSectionTeachingRole TeachingRole,
    Difficulty Difficulty = Difficulty.Unset);

public sealed record MoveAtomicSectionPanelRequest(string Direction);

public sealed record ChangeAtomicSectionItemClassificationRequest(
    AtomicSectionTeachingRole TeachingRole,
    Difficulty Difficulty);

public sealed record CreateTagRequest(string Name, string? Color = null);

public sealed record UpdateTagRequest(string? Name = null, string? Color = null);

public sealed record SetTargetTagsRequest(
    TagBindingTargetType TargetType,
    int TargetId,
    IReadOnlyList<int>? TagIds = null);

public sealed record TeachingNoteBindingRequest(
    TeachingNoteBindingTargetType TargetType,
    int TargetId);

public sealed record CreateTeachingNoteRequest(
    TeachingNoteType NoteType,
    string Content,
    TeachingNoteEffectLevel? EffectLevel = null,
    DateTimeOffset? OccurredAt = null,
    IReadOnlyList<TeachingNoteBindingRequest>? Bindings = null);

public sealed record CreateSectionVariantRequest(
    int SectionId,
    string Title,
    string? Description = null,
    SectionVariantType Type = SectionVariantType.Lecture,
    Difficulty Difficulty = Difficulty.Unset,
    IReadOnlyList<int>? SelectedSectionItemIds = null);

public sealed record PreviewSectionVariantSelectionRequest(
    int SectionId,
    Difficulty Difficulty);

public sealed record AddSectionVariantItemRequest(
    int SectionItemId,
    int SortOrder,
    string? Note = null);

public sealed record CreateHandoutRequest(
    string Title,
    string? Description = null,
    HandoutStatus Status = HandoutStatus.Draft);

public sealed record UpdateHandoutRequest(
    string Title,
    string? Description = null,
    HandoutStatus Status = HandoutStatus.Draft);

public sealed record CreateHandoutVersionRequest(
    string Title,
    string? Description = null,
    HandoutVersionType Type = HandoutVersionType.Normal,
    HandoutVersionStatus Status = HandoutVersionStatus.Draft,
    int SortOrder = 0);

public sealed record UpdateHandoutVersionRequest(
    string Title,
    string? Description = null,
    HandoutVersionType Type = HandoutVersionType.Normal,
    HandoutVersionStatus Status = HandoutVersionStatus.Draft,
    int SortOrder = 0);

public sealed record AddHandoutVersionItemRequest(
    HandoutVersionItemTargetType TargetType,
    int TargetId,
    int SortOrder = 0,
    string? TitleOverride = null,
    string? Note = null,
    int? AfterHandoutVersionItemId = null);

public sealed record BatchAddSectionVariantsToHandoutVersionRequest(
    IReadOnlyList<int>? SectionVariantIds = null,
    int? InsertAfterHandoutVersionItemId = null);

public sealed record MoveHandoutVersionItemRequest(string Direction);

public sealed record UpdateHandoutVersionItemRequest(
    string? TitleOverride = null,
    string? Note = null);

public sealed record CreateOutputTemplateRequest(
    string Title,
    string TemplateDocxPath,
    string? Description = null,
    OutputTemplateStatus Status = OutputTemplateStatus.Active);

public sealed record ValidateOutputTemplateRequest(string TemplateDocxPath);

public sealed record ValidateOutputTemplateResponse(bool Valid, string Message);

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
