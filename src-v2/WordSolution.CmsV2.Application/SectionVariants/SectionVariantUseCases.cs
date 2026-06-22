using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
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
        var title = command.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new CmsV2ApplicationException("SectionVariant title is required.");
        }

        if (command.Difficulty == Difficulty.Unset)
        {
            throw new CmsV2ApplicationException("SectionVariant difficulty cannot be Unset.");
        }

        CreatedEntityResult? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            if (await _unitOfWork.Sections.GetByIdAsync(command.SectionId, transactionCancellationToken) is null)
            {
                throw new CmsV2ApplicationException($"Section {command.SectionId} was not found.");
            }

            var existingVariants = await _unitOfWork.SectionVariants.ListBySectionAsync(command.SectionId, transactionCancellationToken);
            if (existingVariants.Any(item => string.Equals(item.Title, title, StringComparison.OrdinalIgnoreCase)))
            {
                throw new CmsV2ApplicationException("SectionVariant title already exists in this Section.");
            }

            var selectedItems = await ValidateSelectedSectionItemsAsync(
                command.SectionId,
                command.SelectedSectionItemIds ?? [],
                transactionCancellationToken);
            var sortOrder = existingVariants.Count == 0
                ? 1
                : existingVariants.Max(item => item.SortOrder) + 1;
            var variant = new SectionVariant(
                command.SectionId,
                title,
                command.Description,
                command.Type,
                command.Difficulty,
                SectionVariantStatus.Draft,
                sortOrder);

            await _unitOfWork.SectionVariants.AddAsync(variant, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            for (var index = 0; index < selectedItems.Count; index++)
            {
                await _unitOfWork.SectionVariantItems.AddAsync(
                    new SectionVariantItem(variant.Id, selectedItems[index].Id, index + 1),
                    transactionCancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = new CreatedEntityResult(variant.Id);
        }, cancellationToken);

        return result!;
    }

    public async Task<IReadOnlyList<SectionVariantSelectionCandidateDto>> PreviewSectionVariantSelectionAsync(
        PreviewSectionVariantSelectionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Difficulty == Difficulty.Unset)
        {
            throw new CmsV2ApplicationException("SectionVariant preview difficulty cannot be Unset.");
        }

        if (await _unitOfWork.Sections.GetByIdAsync(command.SectionId, cancellationToken) is null)
        {
            throw new CmsV2ApplicationException($"Section {command.SectionId} was not found.");
        }

        var sectionItems = await _unitOfWork.SectionItems.ListBySectionAsync(command.SectionId, cancellationToken);
        var topLevelItems = sectionItems
            .Where(item => item.ParentItemId is null)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id);
        var candidates = new List<SectionVariantSelectionCandidateDto>();

        foreach (var item in topLevelItems)
        {
            var candidate = await BuildSelectionCandidateAsync(item, command.Difficulty, cancellationToken);
            candidates.Add(candidate);
        }

        return candidates;
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

    public async Task DeleteSectionVariantAsync(
        DeleteSectionVariantCommand command,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var variant = await _unitOfWork.SectionVariants.GetByIdAsync(
                    command.SectionVariantId,
                    transactionCancellationToken)
                ?? throw new CmsV2ApplicationException($"SectionVariant {command.SectionVariantId} was not found.");

            var handoutReferences = await _unitOfWork.HandoutVersionItems.ListByTargetAsync(
                HandoutVersionItemTargetType.SectionVariant,
                command.SectionVariantId,
                transactionCancellationToken);
            if (handoutReferences.Count > 0)
            {
                throw new CmsV2ApplicationException("SectionVariant is referenced by HandoutVersion and cannot be deleted.");
            }

            var variantItems = await _unitOfWork.SectionVariantItems.ListBySectionVariantAsync(
                command.SectionVariantId,
                transactionCancellationToken);
            foreach (var item in variantItems)
            {
                _unitOfWork.SectionVariantItems.Remove(item);
            }

            _unitOfWork.SectionVariants.Remove(variant);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
        }, cancellationToken);
    }

    private async Task<SectionVariantSelectionCandidateDto> BuildSelectionCandidateAsync(
        SectionItem item,
        Difficulty selectedDifficulty,
        CancellationToken cancellationToken)
    {
        var resolvedDifficulty = Difficulty.Unset;
        var selectable = item.Status != SectionStatus.Archived;
        var unavailableReason = item.Status == SectionStatus.Archived
            ? "SectionItem is archived."
            : null;

        switch (item.TargetType)
        {
            case SectionItemTargetType.ContentBlock:
                var contentBlock = await _unitOfWork.ContentBlocks.GetByIdAsync(item.TargetId, cancellationToken);
                if (contentBlock is null)
                {
                    selectable = false;
                    unavailableReason ??= "ContentBlock was not found.";
                }
                else
                {
                    resolvedDifficulty = contentBlock.Difficulty;
                    if (contentBlock.Status == ContentBlockStatus.Archived)
                    {
                        selectable = false;
                        unavailableReason ??= "ContentBlock is archived.";
                    }
                }

                break;

            case SectionItemTargetType.AtomicSection:
                var atomicSection = await _unitOfWork.AtomicSections.GetByIdAsync(item.TargetId, cancellationToken);
                if (atomicSection is null)
                {
                    selectable = false;
                    unavailableReason ??= "AtomicSection was not found.";
                }
                else
                {
                    resolvedDifficulty = atomicSection.Difficulty;
                    if (atomicSection.Status == AtomicSectionStatus.Archived)
                    {
                        selectable = false;
                        unavailableReason ??= "AtomicSection is archived.";
                    }
                }

                break;

            default:
                selectable = false;
                unavailableReason ??= "Unsupported SectionItem target type.";
                break;
        }

        var defaultSelected = selectable && IsIncludedByDifficulty(resolvedDifficulty, selectedDifficulty);

        return new SectionVariantSelectionCandidateDto(
            item.Id,
            item.ParentItemId,
            item.SortOrder,
            item.TargetType,
            item.TargetId,
            resolvedDifficulty,
            defaultSelected,
            selectable,
            unavailableReason);
    }

    private static bool IsIncludedByDifficulty(Difficulty candidateDifficulty, Difficulty selectedDifficulty)
    {
        return candidateDifficulty != Difficulty.Unset && candidateDifficulty <= selectedDifficulty;
    }

    private async Task<IReadOnlyList<SectionItem>> ValidateSelectedSectionItemsAsync(
        int sectionId,
        IReadOnlyList<int> selectedSectionItemIds,
        CancellationToken cancellationToken)
    {
        var duplicatedId = selectedSectionItemIds
            .GroupBy(id => id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .FirstOrDefault();
        if (duplicatedId > 0)
        {
            throw new CmsV2ApplicationException($"SectionItem {duplicatedId} was selected more than once.");
        }

        var items = new List<SectionItem>();
        foreach (var sectionItemId in selectedSectionItemIds)
        {
            var item = await _unitOfWork.SectionItems.GetByIdAsync(sectionItemId, cancellationToken)
                ?? throw new CmsV2ApplicationException($"SectionItem {sectionItemId} was not found.");

            if (item.SectionId != sectionId)
            {
                throw new CmsV2ApplicationException("SectionVariantItem must reference a SectionItem from the same Section.");
            }

            if (item.ParentItemId is not null)
            {
                throw new CmsV2ApplicationException("SectionVariant can only select top-level SectionItems.");
            }

            if (item.Status == SectionStatus.Archived)
            {
                throw new CmsV2ApplicationException("SectionVariant cannot select an archived SectionItem.");
            }

            await EnsureSelectedTargetAvailableAsync(item, cancellationToken);
            items.Add(item);
        }

        return items
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToArray();
    }

    private async Task EnsureSelectedTargetAvailableAsync(SectionItem item, CancellationToken cancellationToken)
    {
        switch (item.TargetType)
        {
            case SectionItemTargetType.ContentBlock:
                var contentBlock = await _unitOfWork.ContentBlocks.GetByIdAsync(item.TargetId, cancellationToken)
                    ?? throw new CmsV2ApplicationException($"ContentBlock {item.TargetId} was not found.");
                if (contentBlock.Status == ContentBlockStatus.Archived)
                {
                    throw new CmsV2ApplicationException("SectionVariant cannot select an archived ContentBlock.");
                }

                break;

            case SectionItemTargetType.AtomicSection:
                var atomicSection = await _unitOfWork.AtomicSections.GetByIdAsync(item.TargetId, cancellationToken)
                    ?? throw new CmsV2ApplicationException($"AtomicSection {item.TargetId} was not found.");
                if (atomicSection.Status == AtomicSectionStatus.Archived)
                {
                    throw new CmsV2ApplicationException("SectionVariant cannot select an archived AtomicSection.");
                }

                break;

            default:
                throw new CmsV2ApplicationException("Unsupported SectionItem target type.");
        }
    }
}
