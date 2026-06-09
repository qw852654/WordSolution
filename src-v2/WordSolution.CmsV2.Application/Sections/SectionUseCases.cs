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
}
