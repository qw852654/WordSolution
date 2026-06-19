using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Application.AtomicSections;

public sealed class AtomicSectionUseCases
{
    private static readonly ContentBlockType[] DefaultChildBlockTypes =
    [
        ContentBlockType.KnowledgePoint,
        ContentBlockType.ExampleGroup,
        ContentBlockType.ExerciseGroup
    ];

    private readonly ICmsV2UnitOfWork _unitOfWork;

    public AtomicSectionUseCases(ICmsV2UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<AtomicSection> CreateAtomicSectionAsync(
        CreateAtomicSectionCommand command,
        CancellationToken cancellationToken = default)
    {
        AtomicSection? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            if (await _unitOfWork.Sections.GetByIdAsync(command.SectionId, transactionCancellationToken) is null)
            {
                throw new CmsV2ApplicationException($"Section {command.SectionId} was not found.");
            }

            var atomicSection = new AtomicSection(
                command.SectionId,
                command.Title,
                command.Description,
                command.Type,
                command.Difficulty,
                command.Status);
            await _unitOfWork.AtomicSections.AddAsync(atomicSection, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            for (var index = 0; index < DefaultChildBlockTypes.Length; index++)
            {
                var contentBlock = new ContentBlock(
                    command.SectionId,
                    atomicSection.Title,
                    DefaultChildBlockTypes[index],
                    difficulty: atomicSection.Difficulty,
                    status: ContentBlockStatus.Draft);
                await _unitOfWork.ContentBlocks.AddAsync(contentBlock, transactionCancellationToken);
                await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

                var item = new AtomicSectionItem(
                    atomicSection.Id,
                    contentBlock.Id,
                    ReferenceMode.FollowLatest,
                    lockedContentBlockVersionId: null,
                    sortOrder: (index + 1) * 10);
                await _unitOfWork.AtomicSectionItems.AddAsync(item, transactionCancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = atomicSection;
        }, cancellationToken);

        return result!;
    }

    public async Task<CreatedEntityResult> AddAtomicSectionItemAsync(
        AddAtomicSectionItemCommand command,
        CancellationToken cancellationToken = default)
    {
        CreatedEntityResult? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            if (await _unitOfWork.AtomicSections.GetByIdAsync(command.AtomicSectionId, transactionCancellationToken) is null)
            {
                throw new CmsV2ApplicationException($"AtomicSection {command.AtomicSectionId} was not found.");
            }

            if (await _unitOfWork.ContentBlocks.GetByIdAsync(command.ContentBlockId, transactionCancellationToken) is null)
            {
                throw new CmsV2ApplicationException($"ContentBlock {command.ContentBlockId} was not found.");
            }

            await EnsureLockedVersionBelongsToContentBlockAsync(
                command.LockedContentBlockVersionId,
                command.ContentBlockId,
                transactionCancellationToken);

            var item = new AtomicSectionItem(
                command.AtomicSectionId,
                command.ContentBlockId,
                command.ReferenceMode,
                command.LockedContentBlockVersionId,
                command.SortOrder,
                command.TitleOverride,
                command.Note);

            await _unitOfWork.AtomicSectionItems.AddAsync(item, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = new CreatedEntityResult(item.Id);
        }, cancellationToken);

        return result!;
    }

    public async Task<AtomicSection> RenameAtomicSectionAsync(
        RenameAtomicSectionCommand command,
        CancellationToken cancellationToken = default)
    {
        AtomicSection? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var atomicSection = await _unitOfWork.AtomicSections.GetByIdAsync(
                command.AtomicSectionId,
                transactionCancellationToken);
            if (atomicSection is null)
            {
                throw new CmsV2ApplicationException($"AtomicSection {command.AtomicSectionId} was not found.");
            }

            atomicSection.Rename(command.Title);
            _unitOfWork.AtomicSections.Update(atomicSection);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = atomicSection;
        }, cancellationToken);

        return result!;
    }

    public async Task MoveAtomicSectionItemAsync(
        MoveAtomicSectionItemCommand command,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var item = await GetAtomicSectionItemForCommandAsync(
                command.AtomicSectionId,
                command.AtomicSectionItemId,
                transactionCancellationToken);
            var siblings = (await _unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(
                    command.AtomicSectionId,
                    transactionCancellationToken))
                .OrderBy(candidate => candidate.SortOrder)
                .ThenBy(candidate => candidate.Id)
                .ToList();
            var currentIndex = siblings.FindIndex(candidate => candidate.Id == item.Id);
            var targetIndex = command.Direction == AtomicSectionItemMoveDirection.Up
                ? currentIndex - 1
                : currentIndex + 1;

            if (currentIndex < 0 || targetIndex < 0 || targetIndex >= siblings.Count)
            {
                return;
            }

            siblings.RemoveAt(currentIndex);
            siblings.Insert(targetIndex, item);

            for (var index = 0; index < siblings.Count; index++)
            {
                siblings[index].ChangeSortOrder((index + 1) * 10);
                _unitOfWork.AtomicSectionItems.Update(siblings[index]);
            }

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
        }, cancellationToken);
    }

    public async Task RemoveAtomicSectionItemAsync(
        RemoveAtomicSectionItemCommand command,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var item = await GetAtomicSectionItemForCommandAsync(
                command.AtomicSectionId,
                command.AtomicSectionItemId,
                transactionCancellationToken);

            _unitOfWork.AtomicSectionItems.Remove(item);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
        }, cancellationToken);
    }

    private async Task EnsureLockedVersionBelongsToContentBlockAsync(
        int? contentBlockVersionId,
        int contentBlockId,
        CancellationToken cancellationToken)
    {
        if (!contentBlockVersionId.HasValue)
        {
            return;
        }

        var version = await _unitOfWork.ContentBlockVersions.GetByIdAsync(contentBlockVersionId.Value, cancellationToken);
        if (version is null || version.ContentBlockId != contentBlockId)
        {
            throw new CmsV2ApplicationException("Locked content block version does not belong to the referenced ContentBlock.");
        }
    }

    private async Task<AtomicSectionItem> GetAtomicSectionItemForCommandAsync(
        int atomicSectionId,
        int atomicSectionItemId,
        CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.AtomicSectionItems.GetByIdAsync(atomicSectionItemId, cancellationToken);
        if (item is null || item.AtomicSectionId != atomicSectionId)
        {
            throw new CmsV2ApplicationException(
                $"AtomicSectionItem {atomicSectionItemId} was not found in AtomicSection {atomicSectionId}.");
        }

        return item;
    }
}
