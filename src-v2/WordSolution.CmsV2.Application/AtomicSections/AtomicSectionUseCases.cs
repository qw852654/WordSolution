using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Application.AtomicSections;

public sealed class AtomicSectionUseCases
{
    private readonly ICmsV2UnitOfWork _unitOfWork;

    private static IReadOnlyList<(AtomicSectionTeachingRole TeachingRole, int SortOrder)> DefaultPanelDefinitions { get; } =
    [
        (AtomicSectionTeachingRole.Knowledge, 10),
        (AtomicSectionTeachingRole.Example, 20),
        (AtomicSectionTeachingRole.Variant, 30),
        (AtomicSectionTeachingRole.PreClassQuiz, 40),
    ];

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
            await CreateDefaultPanelsForAtomicSectionAsync(atomicSection, transactionCancellationToken);
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
            _ = await GetAtomicSectionForCommandAsync(command.AtomicSectionId, transactionCancellationToken);
            var contentBlock = await GetContentBlockForCommandAsync(command.ContentBlockId, transactionCancellationToken);

            await EnsureLockedVersionBelongsToContentBlockAsync(
                command.LockedContentBlockVersionId,
                command.ContentBlockId,
                transactionCancellationToken);

            var teachingRole = command.TeachingRole;
            if (command.AtomicSectionPanelId.HasValue)
            {
                var panel = await GetAtomicSectionPanelForCommandAsync(
                    command.AtomicSectionId,
                    command.AtomicSectionPanelId.Value,
                    transactionCancellationToken);
                if (teachingRole == AtomicSectionTeachingRole.Unclassified)
                {
                    teachingRole = panel.TeachingRole;
                }
            }

            var panelId = command.AtomicSectionPanelId
                ?? await ResolvePanelIdAsync(
                    command.AtomicSectionId,
                    teachingRole,
                    contentBlock.Difficulty,
                    transactionCancellationToken);
            var insertIndex = await ResolveItemInsertIndexAsync(
                command.AtomicSectionId,
                panelId,
                command.BeforeAtomicSectionItemId,
                command.AfterAtomicSectionItemId,
                transactionCancellationToken);
            var sortOrder = command.SortOrder ?? await NextItemSortOrderAsync(
                command.AtomicSectionId,
                panelId,
                transactionCancellationToken);

            var item = new AtomicSectionItem(
                command.AtomicSectionId,
                command.ContentBlockId,
                command.ReferenceMode,
                command.LockedContentBlockVersionId,
                sortOrder,
                command.TitleOverride,
                command.Note,
                atomicSectionPanelId: panelId,
                teachingRole: teachingRole);

            await _unitOfWork.AtomicSectionItems.AddAsync(item, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            if (insertIndex.HasValue)
            {
                await ReorderItemInScopeAsync(
                    command.AtomicSectionId,
                    panelId,
                    item.Id,
                    insertIndex.Value,
                    transactionCancellationToken);
                await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            }

            result = new CreatedEntityResult(item.Id);
        }, cancellationToken);

        return result!;
    }

    public async Task<AtomicSectionPanelDto> CreateAtomicSectionPanelAsync(
        CreateAtomicSectionPanelCommand command,
        CancellationToken cancellationToken = default)
    {
        AtomicSectionPanelDto? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            _ = await GetAtomicSectionForCommandAsync(command.AtomicSectionId, transactionCancellationToken);

            var panels = await ListPanelsAsync(command.AtomicSectionId, transactionCancellationToken);
            var insertIndex = ResolvePanelInsertIndex(
                panels,
                command.BeforeAtomicSectionPanelId,
                command.AfterAtomicSectionPanelId);
            var sortOrder = panels.Count == 0 ? 10 : panels.Max(panel => panel.SortOrder) + 10;
            var panel = new AtomicSectionPanel(
                command.AtomicSectionId,
                command.Title,
                command.TeachingRole,
                command.Difficulty,
                sortOrder);

            await _unitOfWork.AtomicSectionPanels.AddAsync(panel, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            await ReassignMatchingUnassignedItemsToPanelAsync(panel, transactionCancellationToken);
            if (insertIndex.HasValue)
            {
                await ReorderPanelInScopeAsync(
                    command.AtomicSectionId,
                    panel.Id,
                    insertIndex.Value,
                    transactionCancellationToken);
            }
            else
            {
                await NormalizePanelSortOrdersAsync(command.AtomicSectionId, transactionCancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = ToPanelDto(panel);
        }, cancellationToken);

        return result!;
    }

    public async Task<AtomicSectionPanelDto> UpdateAtomicSectionPanelAsync(
        UpdateAtomicSectionPanelCommand command,
        CancellationToken cancellationToken = default)
    {
        AtomicSectionPanelDto? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var panel = await GetAtomicSectionPanelForCommandAsync(
                command.AtomicSectionId,
                command.AtomicSectionPanelId,
                transactionCancellationToken);

            panel.Rename(command.Title);
            panel.ChangeClassification(command.TeachingRole, command.Difficulty);
            _unitOfWork.AtomicSectionPanels.Update(panel);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            await ReassignItemsForPanelAsync(panel, transactionCancellationToken);
            await ReassignMatchingUnassignedItemsToPanelAsync(panel, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = ToPanelDto(panel);
        }, cancellationToken);

        return result!;
    }

    public async Task MoveAtomicSectionPanelAsync(
        MoveAtomicSectionPanelCommand command,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var panel = await GetAtomicSectionPanelForCommandAsync(
                command.AtomicSectionId,
                command.AtomicSectionPanelId,
                transactionCancellationToken);
            var panels = await ListPanelsAsync(command.AtomicSectionId, transactionCancellationToken);
            var currentIndex = panels.FindIndex(candidate => candidate.Id == panel.Id);
            var targetIndex = command.Direction == AtomicSectionPanelMoveDirection.Up
                ? currentIndex - 1
                : currentIndex + 1;

            if (currentIndex < 0 || targetIndex < 0 || targetIndex >= panels.Count)
            {
                return;
            }

            panels.RemoveAt(currentIndex);
            panels.Insert(targetIndex, panel);

            for (var index = 0; index < panels.Count; index++)
            {
                panels[index].ChangeSortOrder((index + 1) * 10);
                _unitOfWork.AtomicSectionPanels.Update(panels[index]);
            }

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
        }, cancellationToken);
    }

    public async Task<DeleteAtomicSectionPanelResult> DeleteAtomicSectionPanelAsync(
        DeleteAtomicSectionPanelCommand command,
        CancellationToken cancellationToken = default)
    {
        DeleteAtomicSectionPanelResult? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var panel = await GetAtomicSectionPanelForCommandAsync(
                command.AtomicSectionId,
                command.AtomicSectionPanelId,
                transactionCancellationToken);
            var items = await _unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(
                command.AtomicSectionId,
                transactionCancellationToken);
            var panelItems = items
                .Where(item => item.AtomicSectionPanelId == panel.Id)
                .ToList();

            foreach (var item in panelItems)
            {
                _unitOfWork.AtomicSectionItems.Remove(item);
            }

            _unitOfWork.AtomicSectionPanels.Remove(panel);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = new DeleteAtomicSectionPanelResult(command.AtomicSectionId, panel.Id, panelItems.Count);
        }, cancellationToken);

        return result!;
    }

    public async Task ChangeAtomicSectionItemClassificationAsync(
        ChangeAtomicSectionItemClassificationCommand command,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var item = await GetAtomicSectionItemForCommandAsync(
                command.AtomicSectionId,
                command.AtomicSectionItemId,
                transactionCancellationToken);
            var contentBlock = await GetContentBlockForCommandAsync(item.ContentBlockId, transactionCancellationToken);
            var sourcePanelId = item.AtomicSectionPanelId;
            var targetPanelId = await ResolvePanelIdAsync(
                command.AtomicSectionId,
                command.TeachingRole,
                command.Difficulty,
                transactionCancellationToken);
            var sortOrder = sourcePanelId == targetPanelId
                ? item.SortOrder
                : await NextItemSortOrderAsync(command.AtomicSectionId, targetPanelId, transactionCancellationToken);

            contentBlock.ChangeDifficulty(command.Difficulty);
            item.ChangeClassification(targetPanelId, command.TeachingRole, sortOrder);

            _unitOfWork.ContentBlocks.Update(contentBlock);
            _unitOfWork.AtomicSectionItems.Update(item);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            await NormalizeItemSortOrdersAsync(command.AtomicSectionId, sourcePanelId, transactionCancellationToken);
            if (sourcePanelId != targetPanelId)
            {
                await NormalizeItemSortOrdersAsync(command.AtomicSectionId, targetPanelId, transactionCancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
        }, cancellationToken);
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

    public async Task<AtomicSection> ChangeAtomicSectionDifficultyAsync(
        ChangeAtomicSectionDifficultyCommand command,
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

            atomicSection.ChangeDifficulty(command.Difficulty);
            _unitOfWork.AtomicSections.Update(atomicSection);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = atomicSection;
        }, cancellationToken);

        return result!;
    }

    public async Task<AtomicSection> ChangeAtomicSectionStatusAsync(
        ChangeAtomicSectionStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        AtomicSection? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var atomicSection = await GetAtomicSectionForCommandAsync(
                command.AtomicSectionId,
                transactionCancellationToken);

            atomicSection.ChangeStatus(command.Status);
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
                .Where(candidate => candidate.AtomicSectionPanelId == item.AtomicSectionPanelId)
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

    private async Task<AtomicSection> GetAtomicSectionForCommandAsync(
        int atomicSectionId,
        CancellationToken cancellationToken)
    {
        var atomicSection = await _unitOfWork.AtomicSections.GetByIdAsync(atomicSectionId, cancellationToken);
        if (atomicSection is null)
        {
            throw new CmsV2ApplicationException($"AtomicSection {atomicSectionId} was not found.");
        }

        return atomicSection;
    }

    private async Task CreateDefaultPanelsForAtomicSectionAsync(
        AtomicSection atomicSection,
        CancellationToken cancellationToken)
    {
        var panels = new List<AtomicSectionPanel>();

        foreach (var definition in DefaultPanelDefinitions)
        {
            var panel = new AtomicSectionPanel(
                atomicSection.Id,
                atomicSection.Title,
                definition.TeachingRole,
                atomicSection.Difficulty,
                definition.SortOrder);
            panels.Add(panel);
            await _unitOfWork.AtomicSectionPanels.AddAsync(panel, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var knowledgePanel = panels.Single(panel => panel.TeachingRole == AtomicSectionTeachingRole.Knowledge);
        var knowledgeBlock = new ContentBlock(
            atomicSection.SectionId,
            atomicSection.Title,
            ContentBlockType.KnowledgePoint,
            difficulty: atomicSection.Difficulty);
        await _unitOfWork.ContentBlocks.AddAsync(knowledgeBlock, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _unitOfWork.AtomicSectionItems.AddAsync(
            new AtomicSectionItem(
                atomicSection.Id,
                knowledgeBlock.Id,
                ReferenceMode.FollowLatest,
                lockedContentBlockVersionId: null,
                sortOrder: 10,
                atomicSectionPanelId: knowledgePanel.Id,
                teachingRole: AtomicSectionTeachingRole.Knowledge),
            cancellationToken);
    }

    private async Task<ContentBlock> GetContentBlockForCommandAsync(
        int contentBlockId,
        CancellationToken cancellationToken)
    {
        var contentBlock = await _unitOfWork.ContentBlocks.GetByIdAsync(contentBlockId, cancellationToken);
        if (contentBlock is null)
        {
            throw new CmsV2ApplicationException($"ContentBlock {contentBlockId} was not found.");
        }

        return contentBlock;
    }

    private async Task<AtomicSectionPanel> GetAtomicSectionPanelForCommandAsync(
        int atomicSectionId,
        int atomicSectionPanelId,
        CancellationToken cancellationToken)
    {
        var panel = await _unitOfWork.AtomicSectionPanels.GetByIdAsync(atomicSectionPanelId, cancellationToken);
        if (panel is null || panel.AtomicSectionId != atomicSectionId)
        {
            throw new CmsV2ApplicationException(
                $"AtomicSectionPanel {atomicSectionPanelId} was not found in AtomicSection {atomicSectionId}.");
        }

        return panel;
    }

    private async Task<int?> ResolvePanelIdAsync(
        int atomicSectionId,
        AtomicSectionTeachingRole teachingRole,
        Difficulty difficulty,
        CancellationToken cancellationToken)
    {
        if (teachingRole == AtomicSectionTeachingRole.Unclassified)
        {
            return null;
        }

        var panels = await ListPanelsAsync(atomicSectionId, cancellationToken);
        var exact = panels.FirstOrDefault(panel =>
            panel.TeachingRole == teachingRole && panel.Difficulty == difficulty);
        if (exact is not null)
        {
            return exact.Id;
        }

        var fallback = panels.FirstOrDefault(panel =>
            panel.TeachingRole == teachingRole && panel.Difficulty == Difficulty.Unset);
        return fallback?.Id;
    }

    private async Task ReassignMatchingUnassignedItemsToPanelAsync(
        AtomicSectionPanel panel,
        CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(panel.AtomicSectionId, cancellationToken);
        var unassigned = items
            .Where(item => item.AtomicSectionPanelId is null && item.TeachingRole == panel.TeachingRole)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToList();

        foreach (var item in unassigned)
        {
            var contentBlock = await GetContentBlockForCommandAsync(item.ContentBlockId, cancellationToken);
            if (!PanelMatchesContentBlock(panel, contentBlock))
            {
                continue;
            }

            item.ChangeClassification(panel.Id, item.TeachingRole, await NextItemSortOrderAsync(panel.AtomicSectionId, panel.Id, cancellationToken));
            _unitOfWork.AtomicSectionItems.Update(item);
        }

        await NormalizeItemSortOrdersAsync(panel.AtomicSectionId, panel.Id, cancellationToken);
    }

    private async Task ReassignItemsForPanelAsync(
        AtomicSectionPanel panel,
        CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(panel.AtomicSectionId, cancellationToken);
        var panelItems = items
            .Where(item => item.AtomicSectionPanelId == panel.Id)
            .ToList();

        foreach (var item in panelItems)
        {
            var contentBlock = await GetContentBlockForCommandAsync(item.ContentBlockId, cancellationToken);
            if (item.TeachingRole == panel.TeachingRole && PanelMatchesContentBlock(panel, contentBlock))
            {
                continue;
            }

            var targetPanelId = await ResolvePanelIdAsync(
                panel.AtomicSectionId,
                item.TeachingRole,
                contentBlock.Difficulty,
                cancellationToken);
            var sortOrder = await NextItemSortOrderAsync(panel.AtomicSectionId, targetPanelId, cancellationToken);
            item.ChangeClassification(targetPanelId, item.TeachingRole, sortOrder);
            _unitOfWork.AtomicSectionItems.Update(item);
        }

        await NormalizeItemSortOrdersAsync(panel.AtomicSectionId, panel.Id, cancellationToken);
        await NormalizeItemSortOrdersAsync(panel.AtomicSectionId, null, cancellationToken);
    }

    private static bool PanelMatchesContentBlock(AtomicSectionPanel panel, ContentBlock contentBlock)
    {
        return panel.Difficulty == Difficulty.Unset || panel.Difficulty == contentBlock.Difficulty;
    }

    private async Task<int> NextItemSortOrderAsync(
        int atomicSectionId,
        int? panelId,
        CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSectionId, cancellationToken);
        var maxSortOrder = items
            .Where(item => item.AtomicSectionPanelId == panelId)
            .Select(item => item.SortOrder)
            .DefaultIfEmpty(0)
            .Max();

        return maxSortOrder + 10;
    }

    private async Task<int?> ResolveItemInsertIndexAsync(
        int atomicSectionId,
        int? panelId,
        int? beforeAtomicSectionItemId,
        int? afterAtomicSectionItemId,
        CancellationToken cancellationToken)
    {
        if (beforeAtomicSectionItemId.HasValue && afterAtomicSectionItemId.HasValue)
        {
            throw new CmsV2ApplicationException("Only one AtomicSectionItem insertion anchor can be specified.");
        }

        if (!beforeAtomicSectionItemId.HasValue && !afterAtomicSectionItemId.HasValue)
        {
            return null;
        }

        var siblings = (await _unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSectionId, cancellationToken))
            .Where(item => item.AtomicSectionPanelId == panelId)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToList();

        if (beforeAtomicSectionItemId.HasValue)
        {
            var beforeIndex = siblings.FindIndex(item => item.Id == beforeAtomicSectionItemId.Value);
            if (beforeIndex < 0)
            {
                throw new CmsV2ApplicationException("Before AtomicSectionItem was not found in the target AtomicSection item scope.");
            }

            return beforeIndex;
        }

        var afterIndex = siblings.FindIndex(item => item.Id == afterAtomicSectionItemId!.Value);
        if (afterIndex < 0)
        {
            throw new CmsV2ApplicationException("After AtomicSectionItem was not found in the target AtomicSection item scope.");
        }

        return afterIndex + 1;
    }

    private async Task ReorderItemInScopeAsync(
        int atomicSectionId,
        int? panelId,
        int atomicSectionItemId,
        int insertIndex,
        CancellationToken cancellationToken)
    {
        var siblings = (await _unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSectionId, cancellationToken))
            .Where(item => item.AtomicSectionPanelId == panelId && item.Id != atomicSectionItemId)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToList();
        var inserted = await GetAtomicSectionItemForCommandAsync(
            atomicSectionId,
            atomicSectionItemId,
            cancellationToken);
        var boundedIndex = Math.Clamp(insertIndex, 0, siblings.Count);

        siblings.Insert(boundedIndex, inserted);

        for (var index = 0; index < siblings.Count; index++)
        {
            siblings[index].ChangeSortOrder((index + 1) * 10);
            _unitOfWork.AtomicSectionItems.Update(siblings[index]);
        }
    }

    private async Task NormalizeItemSortOrdersAsync(
        int atomicSectionId,
        int? panelId,
        CancellationToken cancellationToken)
    {
        var items = (await _unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSectionId, cancellationToken))
            .Where(item => item.AtomicSectionPanelId == panelId)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToList();

        for (var index = 0; index < items.Count; index++)
        {
            items[index].ChangeSortOrder((index + 1) * 10);
            _unitOfWork.AtomicSectionItems.Update(items[index]);
        }
    }

    private async Task NormalizePanelSortOrdersAsync(int atomicSectionId, CancellationToken cancellationToken)
    {
        var panels = await ListPanelsAsync(atomicSectionId, cancellationToken);
        for (var index = 0; index < panels.Count; index++)
        {
            panels[index].ChangeSortOrder((index + 1) * 10);
            _unitOfWork.AtomicSectionPanels.Update(panels[index]);
        }
    }

    private static int? ResolvePanelInsertIndex(
        IReadOnlyList<AtomicSectionPanel> panels,
        int? beforeAtomicSectionPanelId,
        int? afterAtomicSectionPanelId)
    {
        if (beforeAtomicSectionPanelId.HasValue && afterAtomicSectionPanelId.HasValue)
        {
            throw new CmsV2ApplicationException("Only one AtomicSectionPanel insertion anchor can be specified.");
        }

        if (!beforeAtomicSectionPanelId.HasValue && !afterAtomicSectionPanelId.HasValue)
        {
            return null;
        }

        if (beforeAtomicSectionPanelId.HasValue)
        {
            var beforeIndex = panels
                .Select((panel, index) => new { panel, index })
                .FirstOrDefault(candidate => candidate.panel.Id == beforeAtomicSectionPanelId.Value)
                ?.index ?? -1;
            if (beforeIndex < 0)
            {
                throw new CmsV2ApplicationException("Before AtomicSectionPanel was not found in the target AtomicSection.");
            }

            return beforeIndex;
        }

        var afterIndex = panels
            .Select((panel, index) => new { panel, index })
            .FirstOrDefault(candidate => candidate.panel.Id == afterAtomicSectionPanelId!.Value)
            ?.index ?? -1;
        if (afterIndex < 0)
        {
            throw new CmsV2ApplicationException("After AtomicSectionPanel was not found in the target AtomicSection.");
        }

        return afterIndex + 1;
    }

    private async Task ReorderPanelInScopeAsync(
        int atomicSectionId,
        int atomicSectionPanelId,
        int insertIndex,
        CancellationToken cancellationToken)
    {
        var panels = (await _unitOfWork.AtomicSectionPanels.ListByAtomicSectionAsync(atomicSectionId, cancellationToken))
            .Where(panel => panel.Id != atomicSectionPanelId)
            .OrderBy(panel => panel.SortOrder)
            .ThenBy(panel => panel.Id)
            .ToList();
        var inserted = await GetAtomicSectionPanelForCommandAsync(
            atomicSectionId,
            atomicSectionPanelId,
            cancellationToken);
        var boundedIndex = Math.Clamp(insertIndex, 0, panels.Count);

        panels.Insert(boundedIndex, inserted);

        for (var index = 0; index < panels.Count; index++)
        {
            panels[index].ChangeSortOrder((index + 1) * 10);
            _unitOfWork.AtomicSectionPanels.Update(panels[index]);
        }
    }

    private async Task<List<AtomicSectionPanel>> ListPanelsAsync(
        int atomicSectionId,
        CancellationToken cancellationToken)
    {
        return (await _unitOfWork.AtomicSectionPanels.ListByAtomicSectionAsync(atomicSectionId, cancellationToken))
            .OrderBy(panel => panel.SortOrder)
            .ThenBy(panel => panel.Id)
            .ToList();
    }

    private static AtomicSectionPanelDto ToPanelDto(AtomicSectionPanel panel)
    {
        return new AtomicSectionPanelDto(
            panel.Id,
            panel.AtomicSectionId,
            panel.Title,
            panel.TeachingRole,
            panel.Difficulty,
            panel.SortOrder);
    }
}
