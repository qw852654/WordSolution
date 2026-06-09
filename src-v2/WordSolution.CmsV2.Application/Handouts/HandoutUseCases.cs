using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Application.Handouts;

public sealed class HandoutUseCases
{
    private readonly ICmsV2UnitOfWork _unitOfWork;

    public HandoutUseCases(ICmsV2UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<CreatedEntityResult> CreateHandoutVersionAsync(
        CreateHandoutVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        CreatedEntityResult? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            if (await _unitOfWork.Handouts.GetByIdAsync(command.HandoutId, transactionCancellationToken) is null)
            {
                throw new CmsV2ApplicationException($"Handout {command.HandoutId} was not found.");
            }

            var version = new HandoutVersion(
                command.HandoutId,
                command.Title,
                command.Description,
                command.Type,
                command.Status,
                command.SortOrder);

            await _unitOfWork.HandoutVersions.AddAsync(version, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = new CreatedEntityResult(version.Id);
        }, cancellationToken);

        return result!;
    }

    public async Task<CreatedEntityResult> AddHandoutVersionItemAsync(
        AddHandoutVersionItemCommand command,
        CancellationToken cancellationToken = default)
    {
        CreatedEntityResult? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            if (await _unitOfWork.HandoutVersions.GetByIdAsync(command.HandoutVersionId, transactionCancellationToken) is null)
            {
                throw new CmsV2ApplicationException($"HandoutVersion {command.HandoutVersionId} was not found.");
            }

            if (command.TargetType == HandoutVersionItemTargetType.SectionVariant)
            {
                if (await _unitOfWork.SectionVariants.GetByIdAsync(command.TargetId, transactionCancellationToken) is null)
                {
                    throw new CmsV2ApplicationException($"SectionVariant {command.TargetId} was not found.");
                }
            }
            else if (command.TargetType == HandoutVersionItemTargetType.ContentBlock)
            {
                if (await _unitOfWork.ContentBlocks.GetByIdAsync(command.TargetId, transactionCancellationToken) is null)
                {
                    throw new CmsV2ApplicationException($"ContentBlock {command.TargetId} was not found.");
                }
            }
            else
            {
                throw new CmsV2ApplicationException("HandoutVersionItem target only allows SectionVariant or ContentBlock.");
            }

            var item = new HandoutVersionItem(
                command.HandoutVersionId,
                command.TargetType,
                command.TargetId,
                command.SortOrder,
                command.TitleOverride,
                command.Note);

            await _unitOfWork.HandoutVersionItems.AddAsync(item, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = new CreatedEntityResult(item.Id);
        }, cancellationToken);

        return result!;
    }
}
