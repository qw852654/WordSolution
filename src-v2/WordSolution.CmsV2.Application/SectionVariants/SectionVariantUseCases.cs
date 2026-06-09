using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Application.SectionVariants;

public sealed class SectionVariantUseCases
{
    private readonly ICmsV2UnitOfWork _unitOfWork;

    public SectionVariantUseCases(ICmsV2UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<CreatedEntityResult> CreateSectionVariantAsync(
        CreateSectionVariantCommand command,
        CancellationToken cancellationToken = default)
    {
        CreatedEntityResult? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            if (await _unitOfWork.Sections.GetByIdAsync(command.SectionId, transactionCancellationToken) is null)
            {
                throw new CmsV2ApplicationException($"Section {command.SectionId} was not found.");
            }

            var variant = new SectionVariant(
                command.SectionId,
                command.Title,
                command.Description,
                command.Type,
                command.Difficulty,
                command.Status,
                command.SortOrder);

            await _unitOfWork.SectionVariants.AddAsync(variant, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = new CreatedEntityResult(variant.Id);
        }, cancellationToken);

        return result!;
    }

    public async Task<CreatedEntityResult> AddSectionVariantItemAsync(
        AddSectionVariantItemCommand command,
        CancellationToken cancellationToken = default)
    {
        CreatedEntityResult? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var variant = await _unitOfWork.SectionVariants.GetByIdAsync(command.SectionVariantId, transactionCancellationToken)
                ?? throw new CmsV2ApplicationException($"SectionVariant {command.SectionVariantId} was not found.");
            var sectionItem = await _unitOfWork.SectionItems.GetByIdAsync(command.SectionItemId, transactionCancellationToken)
                ?? throw new CmsV2ApplicationException($"SectionItem {command.SectionItemId} was not found.");

            if (variant.SectionId != sectionItem.SectionId)
            {
                throw new CmsV2ApplicationException("SectionVariantItem must reference a SectionItem from the same Section.");
            }

            var item = new SectionVariantItem(
                command.SectionVariantId,
                command.SectionItemId,
                command.SortOrder,
                command.Note);

            await _unitOfWork.SectionVariantItems.AddAsync(item, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = new CreatedEntityResult(item.Id);
        }, cancellationToken);

        return result!;
    }
}
