using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Application.ContentBlocks;

public sealed record CreateContentBlockWithBlankDocumentCommand(
    string BankRootDirectory,
    string Title,
    ContentBlockType BlockType,
    string? Summary = null,
    Difficulty Difficulty = Difficulty.Unset,
    QuestionType? QuestionType = null,
    ContentBlockStatus Status = ContentBlockStatus.Draft);

public sealed record ImportContentBlockDocxVersionCommand(
    string BankRootDirectory,
    int ContentBlockId,
    Stream DocxStream,
    bool SetAsCurrent = true);

public sealed record ContentBlockDocumentVersionResult(
    int ContentBlockId,
    int ContentBlockVersionId,
    int VersionNumber,
    string DocxPath,
    string HtmlPreviewPath,
    string PlainTextPath);
