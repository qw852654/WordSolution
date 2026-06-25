using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Application.TeachingNotes;

public sealed record TeachingNoteBindingDto(
    int Id,
    TeachingNoteBindingTargetType TargetType,
    int TargetId,
    DateTimeOffset CreatedTime);

public sealed record TeachingNoteDto(
    int Id,
    TeachingNoteType NoteType,
    string Content,
    TeachingNoteEffectLevel? EffectLevel,
    DateTimeOffset? OccurredAt,
    DateTimeOffset CreatedTime,
    DateTimeOffset UpdatedTime,
    IReadOnlyList<TeachingNoteBindingDto> Bindings);
