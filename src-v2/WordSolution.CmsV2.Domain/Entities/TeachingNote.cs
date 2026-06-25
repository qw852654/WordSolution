using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class TeachingNote
{
    private TeachingNote()
    {
        Content = string.Empty;
    }

    public TeachingNote(
        TeachingNoteType noteType,
        string content,
        TeachingNoteEffectLevel? effectLevel = null,
        DateTimeOffset? occurredAt = null,
        DateTimeOffset? createdTime = null,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.ValidEnum(noteType, nameof(NoteType));
        DomainGuard.NotWhiteSpace(content, nameof(Content));
        if (effectLevel.HasValue)
        {
            DomainGuard.ValidEnum(effectLevel.Value, nameof(EffectLevel));
        }

        NoteType = noteType;
        Content = content.Trim();
        EffectLevel = effectLevel;
        OccurredAt = occurredAt;
        CreatedTime = createdTime ?? DateTimeOffset.UtcNow;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime ?? CreatedTime);
    }

    public int Id { get; private set; }

    public TeachingNoteType NoteType { get; private set; }

    public string Content { get; private set; }

    public TeachingNoteEffectLevel? EffectLevel { get; private set; }

    public DateTimeOffset? OccurredAt { get; private set; }

    public DateTimeOffset CreatedTime { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }

    public void UpdateDetails(
        TeachingNoteType noteType,
        string content,
        TeachingNoteEffectLevel? effectLevel,
        DateTimeOffset? occurredAt,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.ValidEnum(noteType, nameof(NoteType));
        DomainGuard.NotWhiteSpace(content, nameof(Content));
        if (effectLevel.HasValue)
        {
            DomainGuard.ValidEnum(effectLevel.Value, nameof(EffectLevel));
        }

        NoteType = noteType;
        Content = content.Trim();
        EffectLevel = effectLevel;
        OccurredAt = occurredAt;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }
}
