using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Application.TeachingNotes;

public sealed record TeachingNoteBindingCommand(
    TeachingNoteBindingTargetType TargetType,
    int TargetId);

public sealed record CreateTeachingNoteCommand(
    TeachingNoteType NoteType,
    string Content,
    TeachingNoteEffectLevel? EffectLevel,
    DateTimeOffset? OccurredAt,
    IReadOnlyList<TeachingNoteBindingCommand>? Bindings);

public sealed record UpdateTeachingNoteCommand(
    int TeachingNoteId,
    TeachingNoteType NoteType,
    string Content,
    TeachingNoteEffectLevel? EffectLevel,
    DateTimeOffset? OccurredAt,
    IReadOnlyList<TeachingNoteBindingCommand>? Bindings);

public sealed record DeleteTeachingNoteCommand(int TeachingNoteId);

public sealed record SearchTeachingNotesCommand(
    string? Keyword = null,
    TeachingNoteType? NoteType = null,
    TeachingNoteEffectLevel? EffectLevel = null,
    TeachingNoteBindingTargetType? TargetType = null,
    int? TargetId = null,
    DateTimeOffset? OccurredFrom = null,
    DateTimeOffset? OccurredTo = null);
