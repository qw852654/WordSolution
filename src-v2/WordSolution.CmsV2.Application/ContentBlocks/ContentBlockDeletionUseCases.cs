using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Application.ContentBlocks;

public sealed class ContentBlockDeletionUseCases
{
    private readonly ICmsV2UnitOfWork _unitOfWork;
    private readonly IContentBlockFileStore _fileStore;
    private readonly IContentBlockEditSessionStore _editSessionStore;

    public ContentBlockDeletionUseCases(
        ICmsV2UnitOfWork unitOfWork,
        IContentBlockFileStore fileStore,
        IContentBlockEditSessionStore editSessionStore)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
        _editSessionStore = editSessionStore ?? throw new ArgumentNullException(nameof(editSessionStore));
    }

    public async Task<DeleteContentBlockCascadeResult> DeleteContentBlockCascadeAsync(
        DeleteContentBlockCascadeCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.BankRootDirectory))
        {
            throw new CmsV2ApplicationException("Bank root directory is required.");
        }

        var activeSessions = await _editSessionStore.ListActiveAsync(command.BankRootDirectory, cancellationToken);
        if (activeSessions.Any(session => session.ContentBlockId == command.ContentBlockId))
        {
            throw new CmsV2ApplicationException("ContentBlock has an active Word edit session. Sync or cancel it before deleting.");
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var contentBlock = await _unitOfWork.ContentBlocks.GetByIdAsync(
                command.ContentBlockId,
                transactionCancellationToken)
                ?? throw new CmsV2ApplicationException($"ContentBlock {command.ContentBlockId} was not found.");

            if (contentBlock.CurrentVersionId.HasValue)
            {
                contentBlock.ClearCurrentVersion();
                _unitOfWork.ContentBlocks.Update(contentBlock);
                await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            }

            var sectionItems = await _unitOfWork.SectionItems.ListByTargetAsync(
                SectionItemTargetType.ContentBlock,
                command.ContentBlockId,
                transactionCancellationToken);
            var removedSectionVariantItemCount = 0;
            foreach (var sectionItem in sectionItems)
            {
                var variantItems = await _unitOfWork.SectionVariantItems.ListBySectionItemAsync(
                    sectionItem.Id,
                    transactionCancellationToken);
                foreach (var variantItem in variantItems)
                {
                    _unitOfWork.SectionVariantItems.Remove(variantItem);
                    removedSectionVariantItemCount++;
                }

                _unitOfWork.SectionItems.Remove(sectionItem);
            }

            var atomicSectionItems = await _unitOfWork.AtomicSectionItems.ListByContentBlockAsync(
                command.ContentBlockId,
                transactionCancellationToken);
            foreach (var atomicSectionItem in atomicSectionItems)
            {
                _unitOfWork.AtomicSectionItems.Remove(atomicSectionItem);
            }

            var handoutVersionItems = await _unitOfWork.HandoutVersionItems.ListByTargetAsync(
                HandoutVersionItemTargetType.ContentBlock,
                command.ContentBlockId,
                transactionCancellationToken);
            foreach (var handoutVersionItem in handoutVersionItems)
            {
                _unitOfWork.HandoutVersionItems.Remove(handoutVersionItem);
            }

            var parentRelations = await _unitOfWork.ContentBlockRelations.ListParentsAsync(
                command.ContentBlockId,
                transactionCancellationToken);
            var childRelations = await _unitOfWork.ContentBlockRelations.ListChildrenAsync(
                command.ContentBlockId,
                transactionCancellationToken);
            var relations = parentRelations
                .Concat(childRelations)
                .GroupBy(relation => relation.Id)
                .Select(group => group.First())
                .ToArray();
            foreach (var relation in relations)
            {
                _unitOfWork.ContentBlockRelations.Remove(relation);
            }

            var versions = await _unitOfWork.ContentBlockVersions.ListByContentBlockAsync(
                command.ContentBlockId,
                transactionCancellationToken);
            var deletedAssetCount = await DeleteVersionAssetsAsync(versions, transactionCancellationToken);
            foreach (var version in versions)
            {
                _unitOfWork.ContentBlockVersions.Remove(version);
            }

            _unitOfWork.ContentBlocks.Remove(contentBlock);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            return new DeleteContentBlockCascadeResult(
                command.ContentBlockId,
                sectionItems.Count,
                removedSectionVariantItemCount,
                atomicSectionItems.Count,
                relations.Length,
                handoutVersionItems.Count,
                versions.Count,
                deletedAssetCount);
        }, cancellationToken);
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
            if (await _fileStore.ExistsAsync(path, cancellationToken))
            {
                await _fileStore.DeleteIfExistsAsync(path, cancellationToken);
                deletedCount++;
            }
        }

        return deletedCount;
    }
}
