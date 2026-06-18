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

    public async Task<WrapSectionItemsAsAtomicSectionResult> WrapSectionItemsAsAtomicSectionAsync(
        WrapSectionItemsAsAtomicSectionCommand command,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            if (command.SectionItemIds.Count < 2)
            {
                throw new CmsV2ApplicationException("At least two SectionItems are required to wrap as AtomicSection.");
            }

            var selectedIds = command.SectionItemIds.Distinct().ToArray();
            if (selectedIds.Length != command.SectionItemIds.Count)
            {
                throw new CmsV2ApplicationException("SectionItemIds must be distinct.");
            }

            if (await _unitOfWork.Sections.GetByIdAsync(command.SectionId, transactionCancellationToken) is null)
            {
                throw new CmsV2ApplicationException($"Section {command.SectionId} was not found.");
            }

            var allItems = await _unitOfWork.SectionItems.ListBySectionAsync(
                command.SectionId,
                transactionCancellationToken);
            var topLevelItems = allItems
                .Where(item => item.ParentItemId is null)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id)
                .ToList();
            var selectedIdSet = selectedIds.ToHashSet();
            var selectedItems = topLevelItems
                .Where(item => selectedIdSet.Contains(item.Id))
                .ToList();

            if (selectedItems.Count != selectedIds.Length)
            {
                throw new CmsV2ApplicationException("Only top-level SectionItems from the current Section can be wrapped as AtomicSection.");
            }

            if (selectedItems.Any(item => item.TargetType != SectionItemTargetType.ContentBlock))
            {
                throw new CmsV2ApplicationException("Only ContentBlock SectionItems can be wrapped as AtomicSection.");
            }

            var selectedIndexes = selectedItems
                .Select(item => topLevelItems.FindIndex(candidate => candidate.Id == item.Id))
                .Order()
                .ToArray();

            foreach (var item in selectedItems)
            {
                await RequireContentBlockAsync(item.TargetId, transactionCancellationToken);
                await EnsureLockedVersionBelongsToContentBlockAsync(
                    item.LockedContentBlockVersionId,
                    item.TargetId,
                    transactionCancellationToken);
            }

            var sectionVariantReplacementPlans = await BuildSectionVariantReplacementPlansAsync(
                selectedItems,
                selectedIdSet,
                transactionCancellationToken);

            var atomicSection = new AtomicSection(
                command.SectionId,
                command.Title,
                command.Description,
                command.Type,
                command.Difficulty,
                command.Status);
            await _unitOfWork.AtomicSections.AddAsync(atomicSection, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            var atomicSectionItems = new List<AtomicSectionItem>();
            for (var index = 0; index < selectedItems.Count; index++)
            {
                var selectedItem = selectedItems[index];
                var atomicSectionItem = new AtomicSectionItem(
                    atomicSection.Id,
                    selectedItem.TargetId,
                    selectedItem.ReferenceMode,
                    selectedItem.LockedContentBlockVersionId,
                    (index + 1) * 10,
                    selectedItem.TitleOverride,
                    selectedItem.Note);

                await _unitOfWork.AtomicSectionItems.AddAsync(atomicSectionItem, transactionCancellationToken);
                atomicSectionItems.Add(atomicSectionItem);
            }

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            var newSectionItem = new SectionItem(
                command.SectionId,
                SectionItemTargetType.AtomicSection,
                atomicSection.Id,
                ReferenceMode.FollowLatest,
                null,
                selectedItems[0].SortOrder,
                status: SectionStatus.Active);
            await _unitOfWork.SectionItems.AddAsync(newSectionItem, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            foreach (var plan in sectionVariantReplacementPlans)
            {
                foreach (var item in plan.ItemsToReplace)
                {
                    _unitOfWork.SectionVariantItems.Remove(item);
                }

                await _unitOfWork.SectionVariantItems.AddAsync(
                    new SectionVariantItem(
                        plan.SectionVariantId,
                        newSectionItem.Id,
                        plan.SortOrder,
                        plan.Note),
                    transactionCancellationToken);
            }

            foreach (var item in selectedItems)
            {
                _unitOfWork.SectionItems.Remove(item);
            }

            var rewrittenTopLevelItems = topLevelItems
                .Where(item => !selectedIdSet.Contains(item.Id))
                .ToList();
            rewrittenTopLevelItems.Insert(selectedIndexes[0], newSectionItem);

            for (var index = 0; index < rewrittenTopLevelItems.Count; index++)
            {
                rewrittenTopLevelItems[index].ChangeSortOrder((index + 1) * 10);
                _unitOfWork.SectionItems.Update(rewrittenTopLevelItems[index]);
            }

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            return new WrapSectionItemsAsAtomicSectionResult(
                command.SectionId,
                atomicSection.Id,
                newSectionItem.Id,
                selectedItems.Select(item => item.Id).ToArray(),
                atomicSectionItems.Select(item => item.Id).ToArray());
        }, cancellationToken);
    }

    private async Task<IReadOnlyList<SectionVariantReplacementPlan>> BuildSectionVariantReplacementPlansAsync(
        IReadOnlyList<SectionItem> selectedItems,
        IReadOnlySet<int> selectedIdSet,
        CancellationToken cancellationToken)
    {
        var selectedVariantItems = new List<SectionVariantItem>();

        foreach (var item in selectedItems)
        {
            selectedVariantItems.AddRange(
                await _unitOfWork.SectionVariantItems.ListBySectionItemAsync(item.Id, cancellationToken));
        }

        return selectedVariantItems
            .GroupBy(item => item.SectionVariantId)
            .Select(group =>
            {
                var items = group
                    .OrderBy(item => item.SortOrder)
                    .ThenBy(item => item.Id)
                    .ToArray();
                var referencedSelectedIds = items
                    .Select(item => item.SectionItemId)
                    .ToHashSet();

                if (!selectedIdSet.SetEquals(referencedSelectedIds))
                {
                    throw new CmsV2ApplicationException(
                        "SectionVariantItem references only part of the selected SectionItems. Wrap the full referenced range or adjust the SectionVariant first.");
                }

                return new SectionVariantReplacementPlan(
                    group.Key,
                    items,
                    items[0].SortOrder,
                    items[0].Note);
            })
            .ToArray();
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

    private sealed record SectionVariantReplacementPlan(
        int SectionVariantId,
        IReadOnlyList<SectionVariantItem> ItemsToReplace,
        int SortOrder,
        string? Note);
}
