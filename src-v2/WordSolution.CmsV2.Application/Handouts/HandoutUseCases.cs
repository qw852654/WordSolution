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

    public async Task<CreatedEntityResult> CreateHandoutAsync(
        CreateHandoutCommand command,
        CancellationToken cancellationToken = default)
    {
        CreatedEntityResult? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            await EnsureUniqueHandoutTitleAsync(
                command.Title,
                existingHandoutId: null,
                transactionCancellationToken);

            var handout = new Handout(command.Title, command.Description, command.Status);
            await _unitOfWork.Handouts.AddAsync(handout, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = new CreatedEntityResult(handout.Id);
        }, cancellationToken);

        return result!;
    }

    public async Task UpdateHandoutAsync(
        UpdateHandoutCommand command,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var handout = await _unitOfWork.Handouts.GetByIdAsync(command.HandoutId, transactionCancellationToken);
            if (handout is null)
            {
                throw new CmsV2ApplicationException($"Handout {command.HandoutId} was not found.");
            }

            await EnsureUniqueHandoutTitleAsync(
                command.Title,
                handout.Id,
                transactionCancellationToken);

            handout.UpdateDetails(command.Title, command.Description, command.Status);
            _unitOfWork.Handouts.Update(handout);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
        }, cancellationToken);
    }

    public async Task<CreatedEntityResult> CreateHandoutVersionAsync(
        CreateHandoutVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        CreatedEntityResult? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var handout = await _unitOfWork.Handouts.GetByIdAsync(command.HandoutId, transactionCancellationToken);
            if (handout is null)
            {
                throw new CmsV2ApplicationException($"Handout {command.HandoutId} was not found.");
            }

            if (handout.Status == HandoutStatus.Archived)
            {
                throw new CmsV2ApplicationException("Archived Handout cannot create HandoutVersion.");
            }

            await EnsureUniqueHandoutVersionTitleAsync(
                command.HandoutId,
                command.Title,
                existingHandoutVersionId: null,
                transactionCancellationToken);

            var versions = await _unitOfWork.HandoutVersions.ListByHandoutAsync(
                command.HandoutId,
                transactionCancellationToken);
            var nextSortOrder = versions.Count == 0
                ? 10
                : versions.Max(version => version.SortOrder) + 10;

            var version = new HandoutVersion(
                command.HandoutId,
                command.Title,
                command.Description,
                command.Type,
                HandoutVersionStatus.Draft,
                nextSortOrder);

            await _unitOfWork.HandoutVersions.AddAsync(version, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            await CreateDefaultOutputFormForVersionAsync(version.Id, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = new CreatedEntityResult(version.Id);
        }, cancellationToken);

        return result!;
    }

    public async Task UpdateHandoutVersionAsync(
        UpdateHandoutVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var version = await _unitOfWork.HandoutVersions.GetByIdAsync(
                command.HandoutVersionId,
                transactionCancellationToken);
            if (version is null)
            {
                throw new CmsV2ApplicationException($"HandoutVersion {command.HandoutVersionId} was not found.");
            }

            await EnsureUniqueHandoutVersionTitleAsync(
                version.HandoutId,
                command.Title,
                version.Id,
                transactionCancellationToken);

            version.UpdateDetails(
                command.Title,
                command.Description,
                command.Type,
                command.Status,
                command.SortOrder);
            _unitOfWork.HandoutVersions.Update(version);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
        }, cancellationToken);
    }

    public async Task<CreatedEntityResult> AddHandoutVersionItemAsync(
        AddHandoutVersionItemCommand command,
        CancellationToken cancellationToken = default)
    {
        CreatedEntityResult? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            await RequireWritableHandoutVersionAsync(command.HandoutVersionId, transactionCancellationToken);
            await RequireAllowedTargetAsync(command.TargetType, command.TargetId, transactionCancellationToken);

            var siblings = (await _unitOfWork.HandoutVersionItems.ListByHandoutVersionAsync(
                    command.HandoutVersionId,
                    transactionCancellationToken))
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id)
                .ToList();
            var insertIndex = siblings.Count;

            if (command.AfterHandoutVersionItemId.HasValue)
            {
                insertIndex = siblings.FindIndex(item => item.Id == command.AfterHandoutVersionItemId.Value);
                if (insertIndex < 0)
                {
                    throw new CmsV2ApplicationException(
                        $"HandoutVersionItem {command.AfterHandoutVersionItemId.Value} was not found in HandoutVersion {command.HandoutVersionId}.");
                }

                insertIndex++;
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

            siblings.Insert(insertIndex, item);
            NormalizeSortOrder(siblings);

            foreach (var sibling in siblings)
            {
                _unitOfWork.HandoutVersionItems.Update(sibling);
            }

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = new CreatedEntityResult(item.Id);
        }, cancellationToken);

        return result!;
    }

    public async Task<HandoutVersionWorkspaceDto> GetHandoutVersionWorkspaceAsync(
        GetHandoutVersionWorkspaceCommand command,
        CancellationToken cancellationToken = default)
    {
        var version = await _unitOfWork.HandoutVersions.GetByIdAsync(command.HandoutVersionId, cancellationToken);
        if (version is null)
        {
            throw new CmsV2ApplicationException($"HandoutVersion {command.HandoutVersionId} was not found.");
        }

        var handout = await _unitOfWork.Handouts.GetByIdAsync(version.HandoutId, cancellationToken);
        if (handout is null)
        {
            throw new CmsV2ApplicationException($"Handout {version.HandoutId} was not found.");
        }

        var handoutItems = await _unitOfWork.HandoutVersionItems.ListByHandoutVersionAsync(
            version.Id,
            cancellationToken);
        var workspaceItems = new List<HandoutWorkspaceItemDto>();
        foreach (var item in handoutItems)
        {
            workspaceItems.Add(await BuildWorkspaceItemAsync(item, cancellationToken));
        }

        var outputForms = await _unitOfWork.OutputForms.ListByHandoutVersionAsync(version.Id, cancellationToken);
        var generatedFiles = new List<GeneratedFile>();
        foreach (var outputForm in outputForms)
        {
            generatedFiles.AddRange(await _unitOfWork.GeneratedFiles.ListByOutputFormAsync(outputForm.Id, cancellationToken));
        }

        return new HandoutVersionWorkspaceDto(
            handout,
            version,
            workspaceItems,
            outputForms,
            generatedFiles
                .OrderByDescending(file => file.GeneratedTime)
                .ThenByDescending(file => file.Id)
                .ToArray());
    }

    public async Task<IReadOnlyList<SectionVariantSelectionTreeTopicDto>> GetSectionVariantSelectionTreeAsync(
        CancellationToken cancellationToken = default)
    {
        var topics = await _unitOfWork.TeachingTopics.ListAsync(cancellationToken);
        var sections = await _unitOfWork.Sections.ListAsync(cancellationToken);
        var variants = await _unitOfWork.SectionVariants.ListAsync(cancellationToken);

        var topicChildrenByParentId = topics
            .GroupBy(topic => topic.ParentId ?? 0)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(topic => topic.SortOrder)
                    .ThenBy(topic => topic.Id)
                    .ToArray()
                    as IReadOnlyList<TeachingTopic>);
        var sectionsByTopicId = sections
            .GroupBy(section => section.TeachingTopicId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(section => section.SortOrder)
                    .ThenBy(section => section.Id)
                    .ToArray()
                    as IReadOnlyList<Section>);
        var variantsBySectionId = variants
            .GroupBy(variant => variant.SectionId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(variant => variant.SortOrder)
                    .ThenBy(variant => variant.Id)
                    .ToArray()
                    as IReadOnlyList<SectionVariant>);

        return BuildSectionVariantSelectionTree(
            parentTopicId: null,
            topicChildrenByParentId,
            sectionsByTopicId,
            variantsBySectionId);
    }

    public async Task<BatchAddSectionVariantsResult> BatchAddSectionVariantsAsync(
        BatchAddSectionVariantsCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.SectionVariantIds.Count == 0)
        {
            return new BatchAddSectionVariantsResult([], []);
        }

        var duplicateId = command.SectionVariantIds
            .GroupBy(id => id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .FirstOrDefault();
        if (duplicateId > 0)
        {
            throw new CmsV2ApplicationException($"Duplicate SectionVariant id in request: {duplicateId}");
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            await RequireWritableHandoutVersionAsync(command.HandoutVersionId, transactionCancellationToken);

            var allVariants = await _unitOfWork.SectionVariants.ListAsync(transactionCancellationToken);
            var selectedVariantById = allVariants
                .Where(variant => command.SectionVariantIds.Contains(variant.Id))
                .ToDictionary(variant => variant.Id);
            foreach (var variantId in command.SectionVariantIds)
            {
                if (!selectedVariantById.TryGetValue(variantId, out var variant))
                {
                    throw new CmsV2ApplicationException($"SectionVariant {variantId} was not found.");
                }

                if (variant.Status == SectionVariantStatus.Archived)
                {
                    throw new CmsV2ApplicationException($"SectionVariant {variantId} is archived.");
                }
            }

            var siblings = (await _unitOfWork.HandoutVersionItems.ListByHandoutVersionAsync(
                    command.HandoutVersionId,
                    transactionCancellationToken))
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id)
                .ToList();
            var existingVariantIds = siblings
                .Where(item => item.TargetType == HandoutVersionItemTargetType.SectionVariant)
                .Select(item => item.TargetId)
                .ToHashSet();
            var skippedVariantIds = command.SectionVariantIds
                .Where(existingVariantIds.Contains)
                .ToArray();
            var variantsToAdd = selectedVariantById.Values
                .Where(variant => !existingVariantIds.Contains(variant.Id))
                .ToArray();

            if (variantsToAdd.Length == 0)
            {
                return new BatchAddSectionVariantsResult([], skippedVariantIds);
            }

            var insertIndex = siblings.Count;
            if (command.InsertAfterHandoutVersionItemId.HasValue)
            {
                insertIndex = siblings.FindIndex(item => item.Id == command.InsertAfterHandoutVersionItemId.Value);
                if (insertIndex < 0)
                {
                    throw new CmsV2ApplicationException(
                        $"HandoutVersionItem {command.InsertAfterHandoutVersionItemId.Value} was not found in HandoutVersion {command.HandoutVersionId}.");
                }

                insertIndex++;
            }

            var orderedVariantsToAdd = await OrderSectionVariantsBySelectionTreeAsync(
                variantsToAdd,
                transactionCancellationToken);
            var createdItems = new List<HandoutVersionItem>(orderedVariantsToAdd.Count);
            foreach (var variant in orderedVariantsToAdd)
            {
                var item = new HandoutVersionItem(
                    command.HandoutVersionId,
                    HandoutVersionItemTargetType.SectionVariant,
                    variant.Id,
                    sortOrder: 0);
                await _unitOfWork.HandoutVersionItems.AddAsync(item, transactionCancellationToken);
                createdItems.Add(item);
            }

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            siblings.InsertRange(insertIndex, createdItems);
            NormalizeSortOrder(siblings);
            foreach (var sibling in siblings)
            {
                _unitOfWork.HandoutVersionItems.Update(sibling);
            }

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            return new BatchAddSectionVariantsResult(
                createdItems.Select(item => item.Id).ToArray(),
                skippedVariantIds);
        }, cancellationToken);
    }

    public async Task MoveHandoutVersionItemAsync(
        MoveHandoutVersionItemCommand command,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            await RequireWritableHandoutVersionAsync(command.HandoutVersionId, transactionCancellationToken);
            var item = await GetHandoutVersionItemForCommandAsync(
                command.HandoutVersionId,
                command.HandoutVersionItemId,
                transactionCancellationToken);
            var siblings = (await _unitOfWork.HandoutVersionItems.ListByHandoutVersionAsync(
                    command.HandoutVersionId,
                    transactionCancellationToken))
                .OrderBy(candidate => candidate.SortOrder)
                .ThenBy(candidate => candidate.Id)
                .ToList();
            var currentIndex = siblings.FindIndex(candidate => candidate.Id == item.Id);
            var targetIndex = command.Direction == HandoutVersionItemMoveDirection.Up
                ? currentIndex - 1
                : currentIndex + 1;

            if (currentIndex < 0 || targetIndex < 0 || targetIndex >= siblings.Count)
            {
                return;
            }

            siblings.RemoveAt(currentIndex);
            siblings.Insert(targetIndex, item);
            NormalizeSortOrder(siblings);

            foreach (var sibling in siblings)
            {
                _unitOfWork.HandoutVersionItems.Update(sibling);
            }

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
        }, cancellationToken);
    }

    public async Task UpdateHandoutVersionItemAsync(
        UpdateHandoutVersionItemCommand command,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            await RequireWritableHandoutVersionAsync(command.HandoutVersionId, transactionCancellationToken);
            var item = await GetHandoutVersionItemForCommandAsync(
                command.HandoutVersionId,
                command.HandoutVersionItemId,
                transactionCancellationToken);

            item.UpdateDetails(command.TitleOverride, command.Note);
            _unitOfWork.HandoutVersionItems.Update(item);

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
        }, cancellationToken);
    }

    public async Task RemoveHandoutVersionItemAsync(
        RemoveHandoutVersionItemCommand command,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            await RequireWritableHandoutVersionAsync(command.HandoutVersionId, transactionCancellationToken);
            var item = await GetHandoutVersionItemForCommandAsync(
                command.HandoutVersionId,
                command.HandoutVersionItemId,
                transactionCancellationToken);

            _unitOfWork.HandoutVersionItems.Remove(item);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            var siblings = (await _unitOfWork.HandoutVersionItems.ListByHandoutVersionAsync(
                    command.HandoutVersionId,
                    transactionCancellationToken))
                .OrderBy(candidate => candidate.SortOrder)
                .ThenBy(candidate => candidate.Id)
                .ToList();
            NormalizeSortOrder(siblings);

            foreach (var sibling in siblings)
            {
                _unitOfWork.HandoutVersionItems.Update(sibling);
            }

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
        }, cancellationToken);
    }

    private async Task CreateDefaultOutputFormForVersionAsync(
        int handoutVersionId,
        CancellationToken cancellationToken)
    {
        var template = (await _unitOfWork.OutputTemplates.ListAsync(cancellationToken))
            .Where(candidate => candidate.Status == OutputTemplateStatus.Active)
            .OrderBy(candidate => candidate.Id)
            .FirstOrDefault();

        if (template is null)
        {
            return;
        }

        var outputForm = new OutputForm(
            handoutVersionId,
            template.Id,
            "课堂 Word",
            OutputAudience.Student,
            OutputFormat.Word,
            VisibilityMode.Classroom,
            OutputFormStatus.Active,
            sortOrder: 1);

        await _unitOfWork.OutputForms.AddAsync(outputForm, cancellationToken);
    }

    private async Task RequireWritableHandoutVersionAsync(int handoutVersionId, CancellationToken cancellationToken)
    {
        var version = await _unitOfWork.HandoutVersions.GetByIdAsync(handoutVersionId, cancellationToken);
        if (version is null)
        {
            throw new CmsV2ApplicationException($"HandoutVersion {handoutVersionId} was not found.");
        }

        if (version.Status == HandoutVersionStatus.Archived)
        {
            throw new CmsV2ApplicationException("Archived HandoutVersion cannot be modified.");
        }
    }

    private async Task RequireAllowedTargetAsync(
        HandoutVersionItemTargetType targetType,
        int targetId,
        CancellationToken cancellationToken)
    {
        if (targetType == HandoutVersionItemTargetType.SectionVariant)
        {
            if (await _unitOfWork.SectionVariants.GetByIdAsync(targetId, cancellationToken) is null)
            {
                throw new CmsV2ApplicationException($"SectionVariant {targetId} was not found.");
            }
        }
        else if (targetType == HandoutVersionItemTargetType.ContentBlock)
        {
            if (await _unitOfWork.ContentBlocks.GetByIdAsync(targetId, cancellationToken) is null)
            {
                throw new CmsV2ApplicationException($"ContentBlock {targetId} was not found.");
            }
        }
        else if (targetType == HandoutVersionItemTargetType.AtomicSection)
        {
            if (await _unitOfWork.AtomicSections.GetByIdAsync(targetId, cancellationToken) is null)
            {
                throw new CmsV2ApplicationException($"AtomicSection {targetId} was not found.");
            }
        }
        else
        {
            throw new CmsV2ApplicationException("HandoutVersionItem target only allows SectionVariant, ContentBlock or AtomicSection.");
        }
    }

    private async Task<HandoutVersionItem> GetHandoutVersionItemForCommandAsync(
        int handoutVersionId,
        int handoutVersionItemId,
        CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.HandoutVersionItems.GetByIdAsync(handoutVersionItemId, cancellationToken);
        if (item is null || item.HandoutVersionId != handoutVersionId)
        {
            throw new CmsV2ApplicationException(
                $"HandoutVersionItem {handoutVersionItemId} was not found in HandoutVersion {handoutVersionId}.");
        }

        return item;
    }

    private static void NormalizeSortOrder(IReadOnlyList<HandoutVersionItem> items)
    {
        for (var index = 0; index < items.Count; index++)
        {
            items[index].ChangeSortOrder((index + 1) * 10);
        }
    }

    private async Task EnsureUniqueHandoutTitleAsync(
        string title,
        int? existingHandoutId,
        CancellationToken cancellationToken)
    {
        var normalizedTitle = NormalizeTitle(title);
        var handouts = await _unitOfWork.Handouts.ListAsync(cancellationToken);
        if (handouts.Any(handout =>
                handout.Status != HandoutStatus.Archived
                && handout.Id != existingHandoutId
                && string.Equals(NormalizeTitle(handout.Title), normalizedTitle, StringComparison.OrdinalIgnoreCase)))
        {
            throw new CmsV2ApplicationException($"Handout title already exists: {normalizedTitle}");
        }
    }

    private async Task EnsureUniqueHandoutVersionTitleAsync(
        int handoutId,
        string title,
        int? existingHandoutVersionId,
        CancellationToken cancellationToken)
    {
        var normalizedTitle = NormalizeTitle(title);
        var versions = await _unitOfWork.HandoutVersions.ListByHandoutAsync(handoutId, cancellationToken);
        if (versions.Any(version =>
                version.Status != HandoutVersionStatus.Archived
                && version.Id != existingHandoutVersionId
                && string.Equals(NormalizeTitle(version.Title), normalizedTitle, StringComparison.OrdinalIgnoreCase)))
        {
            throw new CmsV2ApplicationException($"HandoutVersion title already exists in Handout {handoutId}: {normalizedTitle}");
        }
    }

    private static string NormalizeTitle(string title)
    {
        return string.IsNullOrWhiteSpace(title) ? string.Empty : title.Trim();
    }

    private static IReadOnlyList<SectionVariantSelectionTreeTopicDto> BuildSectionVariantSelectionTree(
        int? parentTopicId,
        IReadOnlyDictionary<int, IReadOnlyList<TeachingTopic>> topicChildrenByParentId,
        IReadOnlyDictionary<int, IReadOnlyList<Section>> sectionsByTopicId,
        IReadOnlyDictionary<int, IReadOnlyList<SectionVariant>> variantsBySectionId)
    {
        var key = parentTopicId ?? 0;
        if (!topicChildrenByParentId.TryGetValue(key, out var topics))
        {
            return [];
        }

        return topics
            .Select(topic =>
            {
                var sections = sectionsByTopicId.TryGetValue(topic.Id, out var topicSections)
                    ? topicSections
                    : [];
                var sectionNodes = sections
                    .Select(section => new SectionVariantSelectionTreeSectionDto(
                        section,
                        variantsBySectionId.TryGetValue(section.Id, out var sectionVariants)
                            ? sectionVariants
                            : []))
                    .ToArray();

                return new SectionVariantSelectionTreeTopicDto(
                    topic,
                    sectionNodes,
                    BuildSectionVariantSelectionTree(
                        topic.Id,
                        topicChildrenByParentId,
                        sectionsByTopicId,
                        variantsBySectionId));
            })
            .ToArray();
    }

    private async Task<IReadOnlyList<SectionVariant>> OrderSectionVariantsBySelectionTreeAsync(
        IReadOnlyCollection<SectionVariant> variants,
        CancellationToken cancellationToken)
    {
        var variantIds = variants.Select(variant => variant.Id).ToHashSet();
        var variantById = variants.ToDictionary(variant => variant.Id);
        var sections = await _unitOfWork.Sections.ListAsync(cancellationToken);
        var topics = await _unitOfWork.TeachingTopics.ListAsync(cancellationToken);
        var sectionById = sections.ToDictionary(section => section.Id);
        var topicById = topics.ToDictionary(topic => topic.Id);

        return variants
            .OrderBy(variant => BuildTopicSortKey(sectionById.GetValueOrDefault(variant.SectionId), topicById))
            .ThenBy(variant => sectionById.GetValueOrDefault(variant.SectionId)?.SortOrder ?? int.MaxValue)
            .ThenBy(variant => variant.SortOrder)
            .ThenBy(variant => variant.Id)
            .Where(variant => variantIds.Contains(variant.Id) && variantById.ContainsKey(variant.Id))
            .ToArray();
    }

    private static string BuildTopicSortKey(
        Section? section,
        IReadOnlyDictionary<int, TeachingTopic> topicById)
    {
        if (section is null || !topicById.TryGetValue(section.TeachingTopicId, out var topic))
        {
            return string.Empty;
        }

        var path = new Stack<string>();
        var current = topic;
        while (true)
        {
            path.Push($"{current.SortOrder:D8}:{current.Id:D8}");
            if (!current.ParentId.HasValue || !topicById.TryGetValue(current.ParentId.Value, out var parent))
            {
                break;
            }

            current = parent;
        }

        return string.Join("/", path);
    }

    private async Task<HandoutWorkspaceItemDto> BuildWorkspaceItemAsync(
        HandoutVersionItem item,
        CancellationToken cancellationToken)
    {
        var nodeId = $"handout-item:{item.Id}";
        var (sourceTitle, children) = item.TargetType switch
        {
            HandoutVersionItemTargetType.SectionVariant => await BuildSectionVariantTopLevelAsync(
                nodeId,
                item.TargetId,
                cancellationToken),
            HandoutVersionItemTargetType.AtomicSection => await BuildAtomicSectionTopLevelAsync(
                nodeId,
                item.TargetId,
                cancellationToken),
            HandoutVersionItemTargetType.ContentBlock => await BuildContentBlockTopLevelAsync(
                nodeId,
                item.TargetId,
                cancellationToken),
            _ => ("Target object was not found.", Array.Empty<HandoutWorkspaceNodeDto>())
        };

        return new HandoutWorkspaceItemDto(
            nodeId,
            item.Id,
            item.TargetType.ToString(),
            item.TargetId,
            string.IsNullOrWhiteSpace(item.TitleOverride) ? sourceTitle : item.TitleOverride,
            item.TitleOverride,
            item.Note,
            item.SortOrder,
            children);
    }

    private async Task<(string Title, IReadOnlyList<HandoutWorkspaceNodeDto> Children)> BuildSectionVariantTopLevelAsync(
        string path,
        int sectionVariantId,
        CancellationToken cancellationToken)
    {
        var variant = await _unitOfWork.SectionVariants.GetByIdAsync(sectionVariantId, cancellationToken);
        if (variant is null)
        {
            return ("Target object was not found.", Array.Empty<HandoutWorkspaceNodeDto>());
        }

        var variantItems = await _unitOfWork.SectionVariantItems.ListBySectionVariantAsync(sectionVariantId, cancellationToken);
        var children = new List<HandoutWorkspaceNodeDto>();
        foreach (var variantItem in variantItems)
        {
            var sectionItem = await _unitOfWork.SectionItems.GetByIdAsync(variantItem.SectionItemId, cancellationToken);
            if (sectionItem is null)
            {
                children.Add(new HandoutWorkspaceNodeDto(
                    $"{path}/section-variant-item:{variantItem.Id}/missing-section-item:{variantItem.SectionItemId}",
                    "SectionItem",
                    variantItem.SectionItemId,
                    "Target object was not found.",
                    []));
                continue;
            }

            children.Add(await BuildSectionItemNodeAsync(
                $"{path}/section-variant-item:{variantItem.Id}",
                sectionItem,
                cancellationToken));
        }

        return (variant.Title, children);
    }

    private async Task<(string Title, IReadOnlyList<HandoutWorkspaceNodeDto> Children)> BuildAtomicSectionTopLevelAsync(
        string path,
        int atomicSectionId,
        CancellationToken cancellationToken)
    {
        var atomicSection = await _unitOfWork.AtomicSections.GetByIdAsync(atomicSectionId, cancellationToken);
        if (atomicSection is null)
        {
            return ("Target object was not found.", Array.Empty<HandoutWorkspaceNodeDto>());
        }

        return (atomicSection.Title, await BuildAtomicSectionChildrenAsync(path, atomicSection.Id, cancellationToken));
    }

    private async Task<(string Title, IReadOnlyList<HandoutWorkspaceNodeDto> Children)> BuildContentBlockTopLevelAsync(
        string path,
        int contentBlockId,
        CancellationToken cancellationToken)
    {
        var contentBlock = await _unitOfWork.ContentBlocks.GetByIdAsync(contentBlockId, cancellationToken);
        if (contentBlock is null)
        {
            return ("Target object was not found.", Array.Empty<HandoutWorkspaceNodeDto>());
        }

        return (contentBlock.Title, await BuildContentBlockRelationChildrenAsync(
            path,
            contentBlock.Id,
            new HashSet<int> { contentBlock.Id },
            cancellationToken));
    }

    private async Task<HandoutWorkspaceNodeDto> BuildSectionItemNodeAsync(
        string path,
        SectionItem sectionItem,
        CancellationToken cancellationToken)
    {
        if (sectionItem.TargetType == SectionItemTargetType.AtomicSection)
        {
            var atomicSection = await _unitOfWork.AtomicSections.GetByIdAsync(sectionItem.TargetId, cancellationToken);
            if (atomicSection is null)
            {
                return new HandoutWorkspaceNodeDto(
                    $"{path}/section-item:{sectionItem.Id}",
                    "SectionItem",
                    sectionItem.Id,
                    "Target object was not found.",
                    []);
            }

            return new HandoutWorkspaceNodeDto(
                $"{path}/section-item:{sectionItem.Id}",
                "SectionItem",
                sectionItem.Id,
                string.IsNullOrWhiteSpace(sectionItem.TitleOverride) ? atomicSection.Title : sectionItem.TitleOverride,
                await BuildAtomicSectionChildrenAsync(
                    $"{path}/section-item:{sectionItem.Id}/atomic-section:{atomicSection.Id}",
                    atomicSection.Id,
                    cancellationToken));
        }

        var contentBlock = await _unitOfWork.ContentBlocks.GetByIdAsync(sectionItem.TargetId, cancellationToken);
        if (contentBlock is null)
        {
            return new HandoutWorkspaceNodeDto(
                $"{path}/section-item:{sectionItem.Id}",
                "SectionItem",
                sectionItem.Id,
                "Target object was not found.",
                []);
        }

        return new HandoutWorkspaceNodeDto(
            $"{path}/section-item:{sectionItem.Id}",
            "SectionItem",
            sectionItem.Id,
            string.IsNullOrWhiteSpace(sectionItem.TitleOverride) ? contentBlock.Title : sectionItem.TitleOverride,
            await BuildContentBlockRelationChildrenAsync(
                $"{path}/section-item:{sectionItem.Id}/content-block:{contentBlock.Id}",
                contentBlock.Id,
                new HashSet<int> { contentBlock.Id },
                cancellationToken));
    }

    private async Task<IReadOnlyList<HandoutWorkspaceNodeDto>> BuildAtomicSectionChildrenAsync(
        string path,
        int atomicSectionId,
        CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSectionId, cancellationToken);
        var children = new List<HandoutWorkspaceNodeDto>();
        foreach (var item in items)
        {
            var contentBlock = await _unitOfWork.ContentBlocks.GetByIdAsync(item.ContentBlockId, cancellationToken);
            var title = contentBlock is null
                ? "Target object was not found."
                : string.IsNullOrWhiteSpace(item.TitleOverride) ? contentBlock.Title : item.TitleOverride;

            children.Add(new HandoutWorkspaceNodeDto(
                $"{path}/atomic-section-item:{item.Id}",
                "AtomicSectionItem",
                item.Id,
                title,
                contentBlock is null
                    ? []
                    :
                    [
                        new HandoutWorkspaceNodeDto(
                            $"{path}/atomic-section-item:{item.Id}/content-block:{contentBlock.Id}",
                            "ContentBlock",
                            contentBlock.Id,
                            title,
                            await BuildContentBlockRelationChildrenAsync(
                                $"{path}/atomic-section-item:{item.Id}/content-block:{contentBlock.Id}",
                                contentBlock.Id,
                                new HashSet<int> { contentBlock.Id },
                                cancellationToken))
                    ]));
        }

        return children;
    }

    private async Task<IReadOnlyList<HandoutWorkspaceNodeDto>> BuildContentBlockRelationChildrenAsync(
        string path,
        int parentBlockId,
        IReadOnlySet<int> visitedContentBlockIds,
        CancellationToken cancellationToken)
    {
        var relations = await _unitOfWork.ContentBlockRelations.ListChildrenAsync(parentBlockId, cancellationToken);
        var children = new List<HandoutWorkspaceNodeDto>();
        foreach (var relation in relations)
        {
            var childBlock = await _unitOfWork.ContentBlocks.GetByIdAsync(relation.ChildBlockId, cancellationToken);
            if (childBlock is null)
            {
                children.Add(new HandoutWorkspaceNodeDto(
                    $"{path}/content-block-relation:{relation.Id}",
                    "ContentBlockRelation",
                    relation.Id,
                    "Target object was not found.",
                    []));
                continue;
            }

            var nextVisitedIds = visitedContentBlockIds.Append(childBlock.Id).ToHashSet();
            children.Add(new HandoutWorkspaceNodeDto(
                $"{path}/content-block-relation:{relation.Id}",
                "ContentBlockRelation",
                relation.Id,
                string.IsNullOrWhiteSpace(relation.TitleOverride) ? childBlock.Title : relation.TitleOverride,
                visitedContentBlockIds.Contains(childBlock.Id)
                    ? []
                    : await BuildContentBlockRelationChildrenAsync(
                        $"{path}/content-block-relation:{relation.Id}/content-block:{childBlock.Id}",
                        childBlock.Id,
                        nextVisitedIds,
                        cancellationToken)));
        }

        return children;
    }
}
