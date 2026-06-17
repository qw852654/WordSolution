using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Application.AtomicSections;

public sealed class AtomicSectionUseCases
{
    private readonly ICmsV2UnitOfWork _unitOfWork;

    public AtomicSectionUseCases(ICmsV2UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
}
