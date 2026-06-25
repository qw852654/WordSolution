using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Exceptions;
using WordSolution.CmsV2.Domain.Rules;
using QuestionTypeEnum = WordSolution.CmsV2.Domain.Enums.QuestionType;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class ContentBlock
{
    private ContentBlock()
    {
        Title = string.Empty;
    }

    public ContentBlock(
        int sectionId,
        string title,
        ContentBlockType blockType,
        string? summary = null,
        Difficulty difficulty = Difficulty.Unset,
        QuestionType? questionType = null,
        ContentBlockStatus status = ContentBlockStatus.Draft,
        int? currentVersionId = null,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.Positive(sectionId, nameof(SectionId));
        DomainGuard.ValidEnum(blockType, nameof(BlockType));
        DomainGuard.ValidEnum(difficulty, nameof(Difficulty));
        DomainGuard.ValidEnum(status, nameof(Status));
        DomainGuard.PositiveOrNull(currentVersionId, nameof(CurrentVersionId));

        if (questionType.HasValue)
        {
            DomainGuard.ValidEnum(questionType.Value, nameof(QuestionType));
        }

        if (blockType != ContentBlockType.Question && questionType.HasValue && questionType.Value != QuestionTypeEnum.Unset)
        {
            throw new DomainException("QuestionType can only be set when BlockType is Question.");
        }

        SectionId = sectionId;
        Title = title?.Trim() ?? string.Empty;
        Summary = summary?.Trim();
        BlockType = blockType;
        Difficulty = difficulty;
        QuestionType = blockType == ContentBlockType.Question ? questionType ?? QuestionTypeEnum.Unset : null;
        Status = status;
        CurrentVersionId = currentVersionId;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public int SectionId { get; private set; }

    public string Title { get; private set; }

    public string? Summary { get; private set; }

    public ContentBlockType BlockType { get; private set; }

    public Difficulty Difficulty { get; private set; }

    public QuestionType? QuestionType { get; private set; }

    public ContentBlockStatus Status { get; private set; }

    public int? CurrentVersionId { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }

    public void SetCurrentVersion(int contentBlockVersionId, DateTimeOffset? updatedTime = null)
    {
        DomainGuard.Positive(contentBlockVersionId, nameof(contentBlockVersionId));

        CurrentVersionId = contentBlockVersionId;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public void ClearCurrentVersion(DateTimeOffset? updatedTime = null)
    {
        CurrentVersionId = null;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public void ChangeDifficulty(Difficulty difficulty, DateTimeOffset? updatedTime = null)
    {
        DomainGuard.ValidEnum(difficulty, nameof(Difficulty));

        Difficulty = difficulty;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public void Rename(string title, DateTimeOffset? updatedTime = null)
    {
        DomainGuard.NotWhiteSpace(title, nameof(Title));

        Title = title.Trim();
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }
}
