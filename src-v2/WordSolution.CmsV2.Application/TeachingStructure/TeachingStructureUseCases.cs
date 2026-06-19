using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Application.TeachingStructure;

public sealed record CreateTeachingTopicChildCommand(
    int ParentTopicId,
    string Name,
    string? Description = null,
    TeachingTopicStatus Status = TeachingTopicStatus.Active);

public sealed record CreateTeachingTopicNextSiblingCommand(
    int TopicId,
    string Name,
    string? Description = null,
    TeachingTopicStatus Status = TeachingTopicStatus.Active);

public sealed record RenameTeachingTopicCommand(
    int TopicId,
    string Name,
    string? Description = null);

public sealed record DeleteTeachingTopicCommand(int TopicId);

public sealed record CreateSectionForTeachingTopicCommand(
    int TopicId,
    string? Title = null,
    string? Description = null,
    SectionType Type = SectionType.NormalCourse,
    Difficulty Difficulty = Difficulty.Unset,
    SectionStatus Status = SectionStatus.Draft);

public sealed record TeachingStructureNodeDto(
    TeachingTopic TeachingTopic,
    Section? Section,
    IReadOnlyList<SectionVariant> SectionVariants,
    IReadOnlyList<TeachingStructureNodeDto> Children,
    bool IsEmptyTopic,
    bool CanSetDisplayRoot,
    bool CanDelete);

public sealed class TeachingStructureUseCases
{
    private const int SortOrderStep = 10;
    private readonly ICmsV2UnitOfWork _unitOfWork;

    public TeachingStructureUseCases(ICmsV2UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IReadOnlyList<TeachingStructureNodeDto>> GetTeachingStructureAsync(
        CancellationToken cancellationToken = default)
    {
        var topics = await _unitOfWork.TeachingTopics.ListAsync(cancellationToken);
        var sections = await _unitOfWork.Sections.ListAsync(cancellationToken);
        var variants = await _unitOfWork.SectionVariants.ListAsync(cancellationToken);

        var childrenByParentId = topics
            .GroupBy(topic => TopicParentKey(topic.ParentId))
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(topic => topic.SortOrder)
                    .ThenBy(topic => topic.Id)
                    .ToArray()
                    as IReadOnlyList<TeachingTopic>);
        var sectionByTopicId = sections
            .GroupBy(section => section.TeachingTopicId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(section => section.SortOrder)
                    .ThenBy(section => section.Id)
                    .First());
        var variantsBySectionId = variants
            .GroupBy(variant => variant.SectionId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(variant => variant.SortOrder)
                    .ThenBy(variant => variant.Id)
                    .ToArray()
                    as IReadOnlyList<SectionVariant>);

        return BuildNodes(
            parentId: null,
            childrenByParentId,
            sectionByTopicId,
            variantsBySectionId);
    }

    public async Task<TeachingTopic> CreateChildTopicAsync(
        CreateTeachingTopicChildCommand command,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var parent = await _unitOfWork.TeachingTopics.GetByIdAsync(command.ParentTopicId, transactionCancellationToken);
            if (parent is null)
            {
                throw new CmsV2ApplicationException($"TeachingTopic {command.ParentTopicId} was not found.");
            }

            var currentChildren = await _unitOfWork.TeachingTopics.ListChildrenAsync(parent.Id, transactionCancellationToken);
            var topic = new TeachingTopic(
                command.Name,
                command.Description,
                parent.Id,
                NextSortOrder(currentChildren),
                command.Status);
            await _unitOfWork.TeachingTopics.AddAsync(topic, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            var siblings = await NormalizeSiblingSortOrdersAsync(parent.Id, transactionCancellationToken);
            return siblings.Single(sibling => sibling.Id == topic.Id);
        }, cancellationToken);
    }

    public async Task<TeachingTopic> CreateNextSiblingTopicAsync(
        CreateTeachingTopicNextSiblingCommand command,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var current = await _unitOfWork.TeachingTopics.GetByIdAsync(command.TopicId, transactionCancellationToken);
            if (current is null)
            {
                throw new CmsV2ApplicationException($"TeachingTopic {command.TopicId} was not found.");
            }

            var newTopic = new TeachingTopic(
                command.Name,
                command.Description,
                current.ParentId,
                sortOrder: 0,
                command.Status);
            await _unitOfWork.TeachingTopics.AddAsync(newTopic, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            var siblings = await _unitOfWork.TeachingTopics.ListChildrenAsync(current.ParentId, transactionCancellationToken);
            var ordered = new List<TeachingTopic>(siblings.Count);
            foreach (var sibling in siblings.Where(sibling => sibling.Id != newTopic.Id))
            {
                ordered.Add(sibling);
                if (sibling.Id == current.Id)
                {
                    ordered.Add(newTopic);
                }
            }

            if (ordered.All(sibling => sibling.Id != newTopic.Id))
            {
                ordered.Add(newTopic);
            }

            SetSiblingSortOrders(ordered);
            return ordered.Single(sibling => sibling.Id == newTopic.Id);
        }, cancellationToken);
    }

    public async Task<TeachingTopic> RenameTopicAsync(
        RenameTeachingTopicCommand command,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var topic = await _unitOfWork.TeachingTopics.GetByIdAsync(command.TopicId, transactionCancellationToken);
            if (topic is null)
            {
                throw new CmsV2ApplicationException($"TeachingTopic {command.TopicId} was not found.");
            }

            topic.Rename(command.Name, command.Description);
            _unitOfWork.TeachingTopics.Update(topic);

            var boundSections = await _unitOfWork.Sections.ListByTeachingTopicAsync(topic.Id, transactionCancellationToken);
            foreach (var section in boundSections)
            {
                section.Rename(topic.Name);
                _unitOfWork.Sections.Update(section);
            }

            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            return topic;
        }, cancellationToken);
    }

    public async Task DeleteTopicAsync(DeleteTeachingTopicCommand command, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var topic = await _unitOfWork.TeachingTopics.GetByIdAsync(command.TopicId, transactionCancellationToken);
            if (topic is null)
            {
                throw new CmsV2ApplicationException($"TeachingTopic {command.TopicId} was not found.");
            }

            var children = await _unitOfWork.TeachingTopics.ListChildrenAsync(topic.Id, transactionCancellationToken);
            if (children.Count > 0)
            {
                throw new CmsV2ApplicationException("TeachingTopic with child topics cannot be deleted.");
            }

            var sections = await _unitOfWork.Sections.ListByTeachingTopicAsync(topic.Id, transactionCancellationToken);
            if (sections.Count > 0)
            {
                throw new CmsV2ApplicationException("TeachingTopic bound to Section cannot be deleted.");
            }

            _unitOfWork.TeachingTopics.Remove(topic);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            await NormalizeSiblingSortOrdersAsync(topic.ParentId, transactionCancellationToken);
        }, cancellationToken);
    }

    public async Task<Section> CreateSectionForTopicAsync(
        CreateSectionForTeachingTopicCommand command,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var topic = await _unitOfWork.TeachingTopics.GetByIdAsync(command.TopicId, transactionCancellationToken);
            if (topic is null)
            {
                throw new CmsV2ApplicationException($"TeachingTopic {command.TopicId} was not found.");
            }

            var existingSections = await _unitOfWork.Sections.ListByTeachingTopicAsync(topic.Id, transactionCancellationToken);
            if (existingSections.Count > 0)
            {
                throw new CmsV2ApplicationException("TeachingTopic already has a bound Section.");
            }

            var title = string.IsNullOrWhiteSpace(command.Title)
                ? topic.Name
                : command.Title.Trim();
            var section = new Section(
                topic.Id,
                title,
                command.Description,
                command.Type,
                command.Difficulty,
                command.Status,
                sortOrder: SortOrderStep);

            await _unitOfWork.Sections.AddAsync(section, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            return section;
        }, cancellationToken);
    }

    private async Task<IReadOnlyList<TeachingTopic>> NormalizeSiblingSortOrdersAsync(
        int? parentId,
        CancellationToken cancellationToken)
    {
        var siblings = await _unitOfWork.TeachingTopics.ListChildrenAsync(parentId, cancellationToken);
        SetSiblingSortOrders(siblings);

        return siblings;
    }

    private void SetSiblingSortOrders(IReadOnlyList<TeachingTopic> siblings)
    {
        for (var index = 0; index < siblings.Count; index++)
        {
            var sibling = siblings[index];
            var nextSortOrder = (index + 1) * SortOrderStep;
            if (sibling.SortOrder == nextSortOrder)
            {
                continue;
            }

            sibling.SetSortOrder(nextSortOrder);
            _unitOfWork.TeachingTopics.Update(sibling);
        }
    }

    private static IReadOnlyList<TeachingStructureNodeDto> BuildNodes(
        int? parentId,
        IReadOnlyDictionary<int, IReadOnlyList<TeachingTopic>> childrenByParentId,
        IReadOnlyDictionary<int, Section> sectionByTopicId,
        IReadOnlyDictionary<int, IReadOnlyList<SectionVariant>> variantsBySectionId)
    {
        if (!childrenByParentId.TryGetValue(TopicParentKey(parentId), out var topics))
        {
            return [];
        }

        return topics
            .Select(topic =>
            {
                var children = BuildNodes(topic.Id, childrenByParentId, sectionByTopicId, variantsBySectionId);
                sectionByTopicId.TryGetValue(topic.Id, out var section);
                var sectionVariants = section is not null
                    && variantsBySectionId.TryGetValue(section.Id, out var variants)
                        ? variants
                        : [];
                var isEmptyTopic = children.Count == 0 && section is null;
                var canSetDisplayRoot = children.Count > 0 || section is not null;

                return new TeachingStructureNodeDto(
                    topic,
                    section,
                    sectionVariants,
                    children,
                    isEmptyTopic,
                    canSetDisplayRoot,
                    isEmptyTopic);
            })
            .ToArray();
    }

    private static int TopicParentKey(int? parentId)
    {
        return parentId ?? 0;
    }

    private static int NextSortOrder(IReadOnlyList<TeachingTopic> siblings)
    {
        return siblings.Count == 0
            ? SortOrderStep
            : siblings.Max(topic => topic.SortOrder) + SortOrderStep;
    }
}
