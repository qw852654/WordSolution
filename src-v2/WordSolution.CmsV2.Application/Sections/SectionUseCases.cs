using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Application.Sections;

public sealed class SectionUseCases
{
    private readonly ICmsV2UnitOfWork _unitOfWork;

    public SectionUseCases(ICmsV2UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<CreatedEntityResult> AddSectionItemAsync(
        AddSectionItemCommand command,
        CancellationToken cancellationToken = default)
    {
        CreatedEntityResult? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            if (await _unitOfWork.Sections.GetByIdAsync(command.SectionId, transactionCancellationToken) is null)
            {
                throw new CmsV2ApplicationException($"Section {command.SectionId} was not found.");
            }

            if (command.TargetType == SectionItemTargetType.ContentBlock)
            {
                await RequireContentBlockAsync(command.TargetId, transactionCancellationToken);
                await EnsureLockedVersionBelongsToContentBlockAsync(
                    command.LockedContentBlockVersionId,
                    command.TargetId,
                    transactionCancellationToken);
            }
            else
            {
                if (command.ReferenceMode != ReferenceMode.FollowLatest || command.LockedContentBlockVersionId.HasValue)
                {
                    throw new CmsV2ApplicationException("AtomicSection references cannot lock ContentBlock versions.");
                }

                if (await _unitOfWork.AtomicSections.GetByIdAsync(command.TargetId, transactionCancellationToken) is null)
                {
                    throw new CmsV2ApplicationException($"AtomicSection {command.TargetId} was not found.");
                }
            }

            var item = new SectionItem(
                command.SectionId,
                command.TargetType,
                command.TargetId,
                command.ReferenceMode,
                command.LockedContentBlockVersionId,
                command.SortOrder,
                command.TitleOverride,
                command.ParentItemId,
                command.SelectionLayer,
                command.TeachingUseOverride,
                command.Status,
                command.Note);

            await _unitOfWork.SectionItems.AddAsync(item, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = new CreatedEntityResult(item.Id);
        }, cancellationToken);

        return result!;
    }

    public async Task MoveSectionItemAsync(
        MoveSectionItemCommand command,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var item = await GetSectionItemForCommandAsync(
                command.SectionId,
                command.SectionItemId,
                transactionCancellationToken);
            var siblings = (await _unitOfWork.SectionItems.ListBySectionAsync(
                    command.SectionId,
                    transactionCancellationToken))
                .Where(candidate => candidate.ParentItemId == item.ParentItemId)
                .OrderBy(candidate => candidate.SortOrder)
                .ThenBy(candidate => candidate.Id)
                .ToList();
            var currentIndex = siblings.FindIndex(candidate => candidate.Id == item.Id);
            var targetIndex = command.Direction == SectionItemMoveDirection.Up
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
                _unitOfWork.SectionItems.Update(siblings[index]);
            }

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
        }, cancellationToken);
    }

    public async Task RemoveSectionItemAsync(
        RemoveSectionItemCommand command,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var item = await GetSectionItemForCommandAsync(
                command.SectionId,
                command.SectionItemId,
                transactionCancellationToken);

            _unitOfWork.SectionItems.Remove(item);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
        }, cancellationToken);
    }

    private async Task RequireContentBlockAsync(int contentBlockId, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.ContentBlocks.GetByIdAsync(contentBlockId, cancellationToken) is null)
        {
            throw new CmsV2ApplicationException($"ContentBlock {contentBlockId} was not found.");
        }
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

    private async Task<SectionItem> GetSectionItemForCommandAsync(
        int sectionId,
        int sectionItemId,
        CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.SectionItems.GetByIdAsync(sectionItemId, cancellationToken);
        if (item is null || item.SectionId != sectionId)
        {
            throw new CmsV2ApplicationException($"SectionItem {sectionItemId} was not found in Section {sectionId}.");
        }

        return item;
    }
}
