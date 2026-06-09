using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class TeachingNote
{
    public TeachingNote(
        TeachingNoteTargetType targetType,
        int targetId,
        TeachingNoteType noteType,
        string title,
        string content,
        TeachingNoteStatus status = TeachingNoteStatus.Active,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.ValidEnum(targetType, nameof(TargetType));
        DomainGuard.Positive(targetId, nameof(TargetId));
        DomainGuard.ValidEnum(noteType, nameof(NoteType));
        DomainGuard.NotWhiteSpace(title, nameof(Title));
        DomainGuard.NotWhiteSpace(content, nameof(Content));
        DomainGuard.ValidEnum(status, nameof(Status));

        TargetType = targetType;
        TargetId = targetId;
        NoteType = noteType;
        Title = title.Trim();
        Content = content.Trim();
        Status = status;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public TeachingNoteTargetType TargetType { get; private set; }

    public int TargetId { get; private set; }

    public TeachingNoteType NoteType { get; private set; }

    public string Title { get; private set; }

    public string Content { get; private set; }

    public TeachingNoteStatus Status { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }
}
