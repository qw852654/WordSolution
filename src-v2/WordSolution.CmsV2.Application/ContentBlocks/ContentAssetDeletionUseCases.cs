using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Application.ContentBlocks;

public sealed record DeleteSectionItemContentAssetCommand(
    string BankRootDirectory,
    int SectionId,
    int SectionItemId);

public sealed record DeleteAtomicSectionItemContentAssetCommand(
    string BankRootDirectory,
    int AtomicSectionId,
    int AtomicSectionItemId);

public sealed record ContentAssetDeleteResult(
    int RootContentBlockId,
    bool RemovedCurrentReference,
    bool DeletedRootAsset,
    int RemovedSectionItemCount,
    int RemovedSectionVariantItemCount,
    int RemovedAtomicSectionItemCount,
    int RemovedContentBlockRelationCount,
    int DeletedContentBlockCount,
    int DeletedContentBlockVersionCount,
    int DeletedFileCount,
    IReadOnlyList<ContentAssetRetainReasonDto> RetainReasons);

public sealed record ContentAssetRetainReasonDto(
    int ContentBlockId,
    string ReasonCode,
    string Message);

public sealed class ContentAssetDeletionUseCases
{
    private readonly ICmsV2UnitOfWork _unitOfWork;
    private readonly IContentBlockFileStore _fileStore;
    private readonly IContentBlockEditSessionStore _editSessionStore;

    public ContentAssetDeletionUseCases(
        ICmsV2UnitOfWork unitOfWork,
        IContentBlockFileStore fileStore,
        IContentBlockEditSessionStore editSessionStore)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
        _editSessionStore = editSessionStore ?? throw new ArgumentNullException(nameof(editSessionStore));
    }

    public async Task<ContentAssetDeleteResult> DeleteSectionItemContentAssetAsync(
        DeleteSectionItemContentAssetCommand command,
        CancellationToken cancellationToken = default)
    {
        RequireBankRootDirectory(command.BankRootDirectory);

        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var sectionItem = await GetSectionItemForCommandAsync(
                command.SectionId,
                command.SectionItemId,
                transactionCancellationToken);

            if (sectionItem.TargetType == SectionItemTargetType.AtomicSection)
            {
                throw new CmsV2ApplicationException(
                    "Deleting AtomicSection assets from SectionItem is not supported in this phase.");
            }

            var contentBlock = await RequireContentBlockAsync(sectionItem.TargetId, transactionCancellationToken);
            await EnsureNoActiveEditSessionsInGraphAsync(
                command.BankRootDirectory,
                contentBlock.Id,
                transactionCancellationToken);

            var variantItems = await _unitOfWork.SectionVariantItems.ListBySectionItemAsync(
                sectionItem.Id,
                transactionCancellationToken);
            foreach (var variantItem in variantItems)
            {
                _unitOfWork.SectionVariantItems.Remove(variantItem);
            }

            _unitOfWork.SectionItems.Remove(sectionItem);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            return await DeleteUnprotectedRootContentBlockAsync(
                command.BankRootDirectory,
                contentBlock,
                removedSectionItemCount: 1,
                removedSectionVariantItemCount: variantItems.Count,
                removedAtomicSectionItemCount: 0,
                transactionCancellationToken);
        }, cancellationToken);
    }

    public async Task<ContentAssetDeleteResult> DeleteAtomicSectionItemContentAssetAsync(
        DeleteAtomicSectionItemContentAssetCommand command,
        CancellationToken cancellationToken = default)
    {
        RequireBankRootDirectory(command.BankRootDirectory);

        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var atomicSectionItem = await GetAtomicSectionItemForCommandAsync(
                command.AtomicSectionId,
                command.AtomicSectionItemId,
                transactionCancellationToken);
            var contentBlock = await RequireContentBlockAsync(
                atomicSectionItem.ContentBlockId,
                transactionCancellationToken);
            await EnsureNoActiveEditSessionsInGraphAsync(
                command.BankRootDirectory,
                contentBlock.Id,
                transactionCancellationToken);

            _unitOfWork.AtomicSectionItems.Remove(atomicSectionItem);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            return await DeleteUnprotectedRootContentBlockAsync(
                command.BankRootDirectory,
                contentBlock,
                removedSectionItemCount: 0,
                removedSectionVariantItemCount: 0,
                removedAtomicSectionItemCount: 1,
                transactionCancellationToken);
        }, cancellationToken);
    }

    private async Task<ContentAssetDeleteResult> DeleteUnprotectedRootContentBlockAsync(
        string bankRootDirectory,
        ContentBlock contentBlock,
        int removedSectionItemCount,
        int removedSectionVariantItemCount,
        int removedAtomicSectionItemCount,
        CancellationToken cancellationToken)
    {
        var reachableGraph = await BuildReachableGraphAsync(contentBlock.Id, cancellationToken);
        var ignoredRootIncomingRelationIds = reachableGraph.Relations
            .Where(relation => relation.ChildBlockId == contentBlock.Id)
            .Select(relation => relation.Id)
            .ToHashSet();
        var rootRetainReasons = await BuildRetainReasonsAsync(
            contentBlock.Id,
            ignoredRootIncomingRelationIds,
            cancellationToken);
        if (rootRetainReasons.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ContentAssetDeleteResult(
                contentBlock.Id,
                RemovedCurrentReference: true,
                DeletedRootAsset: false,
                removedSectionItemCount,
                removedSectionVariantItemCount,
                removedAtomicSectionItemCount,
                RemovedContentBlockRelationCount: 0,
                DeletedContentBlockCount: 0,
                DeletedContentBlockVersionCount: 0,
                DeletedFileCount: 0,
                rootRetainReasons);
        }

        var plan = await BuildDeletionPlanAsync(contentBlock.Id, cancellationToken);

        foreach (var relation in plan.RelationsToRemove)
        {
            _unitOfWork.ContentBlockRelations.Remove(relation);
        }

        await CleanupBindingsForDeletedBlocksAsync(plan.ContentBlockIdsToDelete, cancellationToken);

        var contentBlocksToDelete = new List<ContentBlock>();
        foreach (var contentBlockId in plan.ContentBlockIdsToDelete)
        {
            var block = await RequireContentBlockAsync(contentBlockId, cancellationToken);
            if (block.CurrentVersionId.HasValue)
            {
                block.ClearCurrentVersion();
                _unitOfWork.ContentBlocks.Update(block);
            }

            contentBlocksToDelete.Add(block);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var versions = new List<ContentBlockVersion>();
        foreach (var contentBlockId in plan.ContentBlockIdsToDelete)
        {
            versions.AddRange(await _unitOfWork.ContentBlockVersions.ListByContentBlockAsync(
                contentBlockId,
                cancellationToken));
        }

        var deletedFileCount = await DeleteVersionAssetsAsync(versions, cancellationToken);
        foreach (var version in versions)
        {
            _unitOfWork.ContentBlockVersions.Remove(version);
        }

        foreach (var block in contentBlocksToDelete)
        {
            _unitOfWork.ContentBlocks.Remove(block);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ContentAssetDeleteResult(
            contentBlock.Id,
            RemovedCurrentReference: true,
            DeletedRootAsset: true,
            removedSectionItemCount,
            removedSectionVariantItemCount,
            removedAtomicSectionItemCount,
            plan.RelationsToRemove.Count,
            plan.ContentBlockIdsToDelete.Count,
            DeletedContentBlockVersionCount: versions.Count,
            deletedFileCount,
            plan.RetainReasons);
    }

    private async Task<ContentAssetDeletionPlan> BuildDeletionPlanAsync(
        int rootContentBlockId,
        CancellationToken cancellationToken)
    {
        var reachable = await BuildReachableGraphAsync(rootContentBlockId, cancellationToken);
        var deleteSet = new HashSet<int> { rootContentBlockId };
        var retainReasons = new Dictionary<int, IReadOnlyList<ContentAssetRetainReasonDto>>();
        var changed = true;

        while (changed)
        {
            changed = false;

            foreach (var relation in reachable.Relations)
            {
                if (!deleteSet.Contains(relation.ParentBlockId) || deleteSet.Contains(relation.ChildBlockId))
                {
                    continue;
                }

                var ignoredIncomingRelationIds = reachable.Relations
                    .Where(candidate =>
                        candidate.ChildBlockId == relation.ChildBlockId &&
                        deleteSet.Contains(candidate.ParentBlockId))
                    .Select(candidate => candidate.Id)
                    .ToHashSet();
                var reasons = await BuildRetainReasonsAsync(
                    relation.ChildBlockId,
                    ignoredIncomingRelationIds,
                    cancellationToken);
                if (reasons.Count == 0)
                {
                    deleteSet.Add(relation.ChildBlockId);
                    retainReasons.Remove(relation.ChildBlockId);
                    changed = true;
                }
                else
                {
                    retainReasons[relation.ChildBlockId] = reasons;
                }
            }
        }

        var relationsToRemove = reachable.Relations
            .Where(relation => deleteSet.Contains(relation.ParentBlockId))
            .GroupBy(relation => relation.Id)
            .Select(group => group.First())
            .ToArray();
        var orderedContentBlockIds = reachable.ContentBlockIds
            .Where(deleteSet.Contains)
            .OrderByDescending(contentBlockId => DistanceFromRoot(reachable, rootContentBlockId, contentBlockId))
            .ThenByDescending(contentBlockId => contentBlockId)
            .ToArray();

        return new ContentAssetDeletionPlan(
            orderedContentBlockIds,
            relationsToRemove,
            retainReasons.Values.SelectMany(reason => reason).ToArray());
    }

    private async Task<ReachableContentBlockGraph> BuildReachableGraphAsync(
        int rootContentBlockId,
        CancellationToken cancellationToken)
    {
        var contentBlockIds = new HashSet<int>();
        var relations = new Dictionary<int, ContentBlockRelation>();
        var queue = new Queue<int>();

        contentBlockIds.Add(rootContentBlockId);
        queue.Enqueue(rootContentBlockId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var childRelations = await _unitOfWork.ContentBlockRelations.ListChildrenAsync(
                currentId,
                cancellationToken);

            foreach (var relation in childRelations)
            {
                relations[relation.Id] = relation;
                if (contentBlockIds.Add(relation.ChildBlockId))
                {
                    queue.Enqueue(relation.ChildBlockId);
                }
            }
        }

        return new ReachableContentBlockGraph(contentBlockIds, relations.Values.ToArray());
    }

    private static int DistanceFromRoot(
        ReachableContentBlockGraph graph,
        int rootContentBlockId,
        int contentBlockId)
    {
        if (contentBlockId == rootContentBlockId)
        {
            return 0;
        }

        var queue = new Queue<(int ContentBlockId, int Distance)>();
        var visited = new HashSet<int> { rootContentBlockId };
        queue.Enqueue((rootContentBlockId, 0));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var relation in graph.Relations.Where(relation => relation.ParentBlockId == current.ContentBlockId))
            {
                if (!visited.Add(relation.ChildBlockId))
                {
                    continue;
                }

                var distance = current.Distance + 1;
                if (relation.ChildBlockId == contentBlockId)
                {
                    return distance;
                }

                queue.Enqueue((relation.ChildBlockId, distance));
            }
        }

        return 0;
    }

    private async Task<IReadOnlyList<ContentAssetRetainReasonDto>> BuildRetainReasonsAsync(
        int contentBlockId,
        IReadOnlySet<int> ignoredIncomingRelationIds,
        CancellationToken cancellationToken)
    {
        var retainReasons = new List<ContentAssetRetainReasonDto>();

        var sectionItems = await _unitOfWork.SectionItems.ListByTargetAsync(
            SectionItemTargetType.ContentBlock,
            contentBlockId,
            cancellationToken);
        if (sectionItems.Count > 0)
        {
            retainReasons.Add(new ContentAssetRetainReasonDto(
                contentBlockId,
                "ReferencedBySection",
                "ContentBlock is still referenced by another SectionItem."));
        }

        var atomicSectionItems = await _unitOfWork.AtomicSectionItems.ListByContentBlockAsync(
            contentBlockId,
            cancellationToken);
        if (atomicSectionItems.Count > 0)
        {
            retainReasons.Add(new ContentAssetRetainReasonDto(
                contentBlockId,
                "ReferencedByAtomicSection",
                "ContentBlock is still referenced by another AtomicSectionItem."));
        }

        var handoutVersionItems = await _unitOfWork.HandoutVersionItems.ListByTargetAsync(
            HandoutVersionItemTargetType.ContentBlock,
            contentBlockId,
            cancellationToken);
        if (handoutVersionItems.Count > 0)
        {
            retainReasons.Add(new ContentAssetRetainReasonDto(
                contentBlockId,
                "ReferencedByHandout",
                "ContentBlock is still directly referenced by a HandoutVersionItem."));
        }

        var parentRelations = await _unitOfWork.ContentBlockRelations.ListParentsAsync(
            contentBlockId,
            cancellationToken);
        if (parentRelations.Any(relation => !ignoredIncomingRelationIds.Contains(relation.Id)))
        {
            retainReasons.Add(new ContentAssetRetainReasonDto(
                contentBlockId,
                "ReferencedByRelation",
                "ContentBlock is still referenced by another ContentBlockRelation."));
        }

        return retainReasons;
    }

    private async Task CleanupBindingsForDeletedBlocksAsync(
        IReadOnlyList<int> contentBlockIds,
        CancellationToken cancellationToken)
    {
        var teachingNoteIdsToCheck = new HashSet<int>();

        foreach (var contentBlockId in contentBlockIds)
        {
            var tagBindings = await _unitOfWork.TagBindings.ListByTargetAsync(
                TagBindingTargetType.ContentBlock,
                contentBlockId,
                cancellationToken);
            foreach (var tagBinding in tagBindings)
            {
                _unitOfWork.TagBindings.Remove(tagBinding);
            }

            var teachingNoteBindings = await _unitOfWork.TeachingNoteBindings.ListByTargetAsync(
                TeachingNoteBindingTargetType.ContentBlock,
                contentBlockId,
                cancellationToken);
            foreach (var teachingNoteBinding in teachingNoteBindings)
            {
                teachingNoteIdsToCheck.Add(teachingNoteBinding.TeachingNoteId);
                _unitOfWork.TeachingNoteBindings.Remove(teachingNoteBinding);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var teachingNoteId in teachingNoteIdsToCheck)
        {
            var remainingBindings = await _unitOfWork.TeachingNoteBindings.ListByTeachingNoteAsync(
                teachingNoteId,
                cancellationToken);
            if (remainingBindings.Count > 0)
            {
                continue;
            }

            var teachingNote = await _unitOfWork.TeachingNotes.GetByIdAsync(teachingNoteId, cancellationToken);
            if (teachingNote is not null)
            {
                _unitOfWork.TeachingNotes.Remove(teachingNote);
            }
        }
    }

    private async Task<int> DeleteVersionAssetsAsync(
        IReadOnlyList<ContentBlockVersion> versions,
        CancellationToken cancellationToken)
    {
        var paths = versions
            .SelectMany(version => new[] { version.DocxPath, version.HtmlPreviewPath })
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => path!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var deletedCount = 0;

        foreach (var path in paths)
        {
            try
            {
                if (!await _fileStore.ExistsAsync(path, cancellationToken))
                {
                    continue;
                }

                await _fileStore.DeleteIfExistsAsync(path, cancellationToken);
            }
            catch (Exception exception)
            {
                throw new CmsV2ApplicationException($"Failed to delete ContentBlock asset file: {path}", exception);
            }

            deletedCount++;
        }

        return deletedCount;
    }

    private async Task EnsureNoActiveEditSessionsInGraphAsync(
        string bankRootDirectory,
        int rootContentBlockId,
        CancellationToken cancellationToken)
    {
        var reachable = await BuildReachableGraphAsync(rootContentBlockId, cancellationToken);
        var activeSessions = await _editSessionStore.ListActiveAsync(bankRootDirectory, cancellationToken);
        if (activeSessions.Any(session => reachable.ContentBlockIds.Contains(session.ContentBlockId)))
        {
            throw new CmsV2ApplicationException(
                "ContentBlock has an active Word edit session. Sync or cancel it before deleting.");
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

    private async Task<ContentBlock> RequireContentBlockAsync(
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

    private static void RequireBankRootDirectory(string bankRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(bankRootDirectory))
        {
            throw new CmsV2ApplicationException("Bank root directory is required.");
        }
    }

    private sealed record ReachableContentBlockGraph(
        IReadOnlySet<int> ContentBlockIds,
        IReadOnlyList<ContentBlockRelation> Relations);

    private sealed record ContentAssetDeletionPlan(
        IReadOnlyList<int> ContentBlockIdsToDelete,
        IReadOnlyList<ContentBlockRelation> RelationsToRemove,
        IReadOnlyList<ContentAssetRetainReasonDto> RetainReasons);
}
