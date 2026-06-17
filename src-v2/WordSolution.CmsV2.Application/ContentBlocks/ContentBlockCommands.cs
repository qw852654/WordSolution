using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Application.ContentBlocks;

public sealed record CreateContentBlockWithInitialVersionCommand(
    int SectionId,
    string Title,
    ContentBlockType BlockType,
    string DocxPath,
    string? Summary = null,
    Difficulty Difficulty = Difficulty.Unset,
    QuestionType? QuestionType = null,
    ContentBlockStatus Status = ContentBlockStatus.Draft,
    string? HtmlPreviewPath = null,
    string? PlainText = null);

public sealed record CreateContentBlockVersionCommand(
    int ContentBlockId,
    string DocxPath,
    string? HtmlPreviewPath = null,
    string? PlainText = null,
    bool SetAsCurrent = false);

public sealed record SetCurrentContentBlockVersionCommand(
    int ContentBlockId,
    int ContentBlockVersionId);

public sealed record AddContentBlockRelationCommand(
    int ParentBlockId,
    int ChildBlockId,
    ReferenceMode ReferenceMode,
    int? LockedContentBlockVersionId,
    int SortOrder,
    string? TitleOverride = null,
    string? Note = null);
