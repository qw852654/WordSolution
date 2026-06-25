using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Application.TeachingNotes;

public sealed class TeachingNoteUseCases
{
    private readonly ICmsV2UnitOfWork _unitOfWork;

    public TeachingNoteUseCases(ICmsV2UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<TeachingNoteDto> GetTeachingNoteAsync(
        int teachingNoteId,
        CancellationToken cancellationToken = default)
    {
        var note = await RequireTeachingNoteAsync(teachingNoteId, cancellationToken);

        return await ToDtoAsync(note, cancellationToken);
    }

    public async Task<IReadOnlyList<TeachingNoteDto>> ListTeachingNotesAsync(
        SearchTeachingNotesCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.TargetId.HasValue && !command.TargetType.HasValue)
        {
            throw new CmsV2ApplicationException("TargetType is required when TargetId is provided.");
        }

        if (command.TargetType.HasValue && command.TargetId.HasValue)
        {
            await RequireTargetAsync(command.TargetType.Value, command.TargetId.Value, cancellationToken);
        }

        var notes = command.TargetType.HasValue && command.TargetId.HasValue
            ? await _unitOfWork.TeachingNotes.ListByTargetAsync(
                command.TargetType.Value,
                command.TargetId.Value,
                cancellationToken)
            : await _unitOfWork.TeachingNotes.SearchAsync(
                command.Keyword,
                command.NoteType,
                command.EffectLevel,
                command.OccurredFrom,
                command.OccurredTo,
                cancellationToken);

        var filteredNotes = await FilterNotesBySearchCommandAsync(notes, command, cancellationToken);
        var results = new List<TeachingNoteDto>(filteredNotes.Count);
        foreach (var note in filteredNotes)
        {
            results.Add(await ToDtoAsync(note, cancellationToken));
        }

        return results;
    }

    public async Task<TeachingNoteDto> CreateTeachingNoteAsync(
        CreateTeachingNoteCommand command,
        CancellationToken cancellationToken = default)
    {
        RequireContent(command.Content);
        var bindings = await NormalizeAndValidateBindingsAsync(command.Bindings, cancellationToken);
        TeachingNote note = null!;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            note = new TeachingNote(command.NoteType, command.Content, command.EffectLevel, command.OccurredAt);
            await _unitOfWork.TeachingNotes.AddAsync(note, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            foreach (var binding in bindings)
            {
                await _unitOfWork.TeachingNoteBindings.AddAsync(
                    new TeachingNoteBinding(note.Id, binding.TargetType, binding.TargetId),
                    transactionCancellationToken);
            }
        }, cancellationToken);

        return await GetTeachingNoteAsync(note.Id, cancellationToken);
    }

    public async Task<TeachingNoteDto> UpdateTeachingNoteAsync(
        UpdateTeachingNoteCommand command,
        CancellationToken cancellationToken = default)
    {
        RequireContent(command.Content);
        var bindings = await NormalizeAndValidateBindingsAsync(command.Bindings, cancellationToken);

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var note = await RequireTeachingNoteAsync(command.TeachingNoteId, transactionCancellationToken);
            note.UpdateDetails(command.NoteType, command.Content, command.EffectLevel, command.OccurredAt);
            _unitOfWork.TeachingNotes.Update(note);

            var existingBindings = await _unitOfWork.TeachingNoteBindings.ListByTeachingNoteAsync(
                command.TeachingNoteId,
                transactionCancellationToken);
            foreach (var binding in existingBindings)
            {
                _unitOfWork.TeachingNoteBindings.Remove(binding);
            }

            foreach (var binding in bindings)
            {
                await _unitOfWork.TeachingNoteBindings.AddAsync(
                    new TeachingNoteBinding(command.TeachingNoteId, binding.TargetType, binding.TargetId),
                    transactionCancellationToken);
            }
        }, cancellationToken);

        return await GetTeachingNoteAsync(command.TeachingNoteId, cancellationToken);
    }

    public async Task DeleteTeachingNoteAsync(
        DeleteTeachingNoteCommand command,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var note = await RequireTeachingNoteAsync(command.TeachingNoteId, transactionCancellationToken);
            _unitOfWork.TeachingNotes.Remove(note);
        }, cancellationToken);
    }

    private async Task<IReadOnlyList<TeachingNote>> FilterNotesBySearchCommandAsync(
        IReadOnlyList<TeachingNote> notes,
        SearchTeachingNotesCommand command,
        CancellationToken cancellationToken)
    {
        IEnumerable<TeachingNote> query = notes;

        if (command.TargetType.HasValue && !command.TargetId.HasValue)
        {
            var matchingBindings = await _unitOfWork.TeachingNoteBindings.ListAsync(cancellationToken);
            var matchingNoteIds = matchingBindings
                .Where(binding => binding.TargetType == command.TargetType.Value)
                .Select(binding => binding.TeachingNoteId)
                .ToHashSet();

            query = query.Where(note => matchingNoteIds.Contains(note.Id));
        }

        if (command.TargetType.HasValue && command.TargetId.HasValue)
        {
            query = query.Where(note =>
                string.IsNullOrWhiteSpace(command.Keyword)
                || note.Content.Contains(command.Keyword.Trim(), StringComparison.Ordinal));

            if (command.NoteType.HasValue)
            {
                query = query.Where(note => note.NoteType == command.NoteType.Value);
            }

            if (command.EffectLevel.HasValue)
            {
                query = query.Where(note => note.EffectLevel == command.EffectLevel.Value);
            }

            if (command.OccurredFrom.HasValue)
            {
                query = query.Where(note => note.OccurredAt.HasValue && note.OccurredAt.Value >= command.OccurredFrom.Value);
            }

            if (command.OccurredTo.HasValue)
            {
                query = query.Where(note => note.OccurredAt.HasValue && note.OccurredAt.Value <= command.OccurredTo.Value);
            }
        }

        return query
            .OrderByDescending(note => note.UpdatedTime)
            .ThenByDescending(note => note.Id)
            .ToArray();
    }

    private async Task<TeachingNote> RequireTeachingNoteAsync(
        int teachingNoteId,
        CancellationToken cancellationToken)
    {
        if (teachingNoteId <= 0)
        {
            throw new CmsV2ApplicationException("TeachingNoteId must be greater than 0.");
        }

        return await _unitOfWork.TeachingNotes.GetByIdAsync(teachingNoteId, cancellationToken)
            ?? throw new CmsV2ApplicationException($"TeachingNote {teachingNoteId} was not found.");
    }

    private static void RequireContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new CmsV2ApplicationException("Teaching note content cannot be empty.");
        }
    }

    private async Task<IReadOnlyList<TeachingNoteBindingCommand>> NormalizeAndValidateBindingsAsync(
        IReadOnlyList<TeachingNoteBindingCommand>? bindings,
        CancellationToken cancellationToken)
    {
        if (bindings is null || bindings.Count == 0)
        {
            throw new CmsV2ApplicationException("At least one teaching note binding is required.");
        }

        var results = new List<TeachingNoteBindingCommand>(bindings.Count);
        var seen = new HashSet<(TeachingNoteBindingTargetType TargetType, int TargetId)>();
        foreach (var binding in bindings)
        {
            if (binding.TargetId <= 0)
            {
                throw new CmsV2ApplicationException("TargetId must be greater than 0.");
            }

            if (!Enum.IsDefined(binding.TargetType))
            {
                throw new CmsV2ApplicationException("Unsupported teaching note binding target type.");
            }

            if (!seen.Add((binding.TargetType, binding.TargetId)))
            {
                throw new CmsV2ApplicationException("A teaching note cannot bind the same target more than once.");
            }

            await RequireTargetAsync(binding.TargetType, binding.TargetId, cancellationToken);
            results.Add(binding);
        }

        return results;
    }

    private async Task RequireTargetAsync(
        TeachingNoteBindingTargetType targetType,
        int targetId,
        CancellationToken cancellationToken)
    {
        if (targetId <= 0)
        {
            throw new CmsV2ApplicationException("TargetId must be greater than 0.");
        }

        var exists = targetType switch
        {
            TeachingNoteBindingTargetType.ContentBlock => await _unitOfWork.ContentBlocks.GetByIdAsync(targetId, cancellationToken) is not null,
            TeachingNoteBindingTargetType.Section => await _unitOfWork.Sections.GetByIdAsync(targetId, cancellationToken) is not null,
            TeachingNoteBindingTargetType.AtomicSection => await _unitOfWork.AtomicSections.GetByIdAsync(targetId, cancellationToken) is not null,
            TeachingNoteBindingTargetType.AtomicSectionPanel => await _unitOfWork.AtomicSectionPanels.GetByIdAsync(targetId, cancellationToken) is not null,
            TeachingNoteBindingTargetType.AtomicSectionItem => await _unitOfWork.AtomicSectionItems.GetByIdAsync(targetId, cancellationToken) is not null,
            TeachingNoteBindingTargetType.SectionItem => await _unitOfWork.SectionItems.GetByIdAsync(targetId, cancellationToken) is not null,
            _ => throw new CmsV2ApplicationException("Unsupported teaching note binding target type.")
        };

        if (!exists)
        {
            throw new CmsV2ApplicationException($"{targetType} target {targetId} was not found.");
        }
    }

    private async Task<TeachingNoteDto> ToDtoAsync(
        TeachingNote note,
        CancellationToken cancellationToken)
    {
        var bindings = await _unitOfWork.TeachingNoteBindings.ListByTeachingNoteAsync(note.Id, cancellationToken);

        return new TeachingNoteDto(
            note.Id,
            note.NoteType,
            note.Content,
            note.EffectLevel,
            note.OccurredAt,
            note.CreatedTime,
            note.UpdatedTime,
            bindings
                .Select(binding => new TeachingNoteBindingDto(
                    binding.Id,
                    binding.TargetType,
                    binding.TargetId,
                    binding.CreatedTime))
                .ToArray());
    }
}
