using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Application.ContentBlocks;

public sealed record CreateContentBlockCommand(
    int SectionId,
    string Title,
    ContentBlockType BlockType,
    Difficulty Difficulty = Difficulty.Unset,
    string? Summary = null,
    QuestionType? QuestionType = null,
    ContentBlockStatus Status = ContentBlockStatus.Draft);

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

public sealed record ChangeContentBlockDifficultyCommand(
    int ContentBlockId,
    Difficulty Difficulty);

public sealed record SearchContentBlocksCommand(
    IReadOnlyList<int>? TagIds = null);

public sealed record AddContentBlockRelationCommand(
    int ParentBlockId,
    int ChildBlockId,
    ReferenceMode ReferenceMode,
    int? LockedContentBlockVersionId,
    int SortOrder,
    string? TitleOverride = null,
    string? Note = null);

public enum ContentBlockRelationMoveDirection
{
    Up,
    Down
}

public sealed record MoveContentBlockRelationCommand(
    int ParentBlockId,
    int RelationId,
    ContentBlockRelationMoveDirection Direction);

public sealed record RemoveContentBlockRelationCommand(
    int ParentBlockId,
    int RelationId);

public sealed record DeleteContentBlockCascadeCommand(
    string BankRootDirectory,
    int ContentBlockId);

public sealed record DeleteContentBlockCascadeResult(
    int ContentBlockId,
    int RemovedSectionItemCount,
    int RemovedSectionVariantItemCount,
    int RemovedAtomicSectionItemCount,
    int RemovedContentBlockRelationCount,
    int RemovedHandoutVersionItemCount,
    int RemovedVersionCount,
    int DeletedAssetCount);
