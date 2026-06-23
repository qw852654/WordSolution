using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Domain.Repositories;

public interface ITeachingTopicRepository : IRepository<TeachingTopic>
{
    Task<IReadOnlyList<TeachingTopic>> ListChildrenAsync(int? parentId, CancellationToken cancellationToken = default);
}

public interface ISectionRepository : IRepository<Section>
{
    Task<IReadOnlyList<Section>> ListByTeachingTopicAsync(int teachingTopicId, CancellationToken cancellationToken = default);
}

public interface ISectionItemRepository : IRepository<SectionItem>
{
    Task<IReadOnlyList<SectionItem>> ListBySectionAsync(int sectionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SectionItem>> ListByTargetAsync(
        SectionItemTargetType targetType,
        int targetId,
        CancellationToken cancellationToken = default);
}

public interface IAtomicSectionRepository : IRepository<AtomicSection>
{
}

public interface IAtomicSectionPanelRepository : IRepository<AtomicSectionPanel>
{
    Task<IReadOnlyList<AtomicSectionPanel>> ListByAtomicSectionAsync(
        int atomicSectionId,
        CancellationToken cancellationToken = default);
}

public interface IAtomicSectionItemRepository : IRepository<AtomicSectionItem>
{
    Task<IReadOnlyList<AtomicSectionItem>> ListByAtomicSectionAsync(
        int atomicSectionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AtomicSectionItem>> ListByContentBlockAsync(
        int contentBlockId,
        CancellationToken cancellationToken = default);
}

public interface ISectionVariantRepository : IRepository<SectionVariant>
{
    Task<IReadOnlyList<SectionVariant>> ListBySectionAsync(int sectionId, CancellationToken cancellationToken = default);
}

public interface ISectionVariantItemRepository : IRepository<SectionVariantItem>
{
    Task<IReadOnlyList<SectionVariantItem>> ListBySectionVariantAsync(
        int sectionVariantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SectionVariantItem>> ListBySectionItemAsync(
        int sectionItemId,
        CancellationToken cancellationToken = default);
}

public interface IContentBlockRepository : IRepository<ContentBlock>
{
}

public interface IContentBlockVersionRepository : IRepository<ContentBlockVersion>
{
    Task<IReadOnlyList<ContentBlockVersion>> ListByContentBlockAsync(
        int contentBlockId,
        CancellationToken cancellationToken = default);

    Task<ContentBlockVersion?> GetByContentBlockAndVersionNumberAsync(
        int contentBlockId,
        int versionNumber,
        CancellationToken cancellationToken = default);

    Task<ContentBlockVersion?> GetCurrentByContentBlockAsync(
        int contentBlockId,
        CancellationToken cancellationToken = default);
}

public interface IContentBlockRelationRepository : IRepository<ContentBlockRelation>
{
    Task<IReadOnlyList<ContentBlockRelation>> ListChildrenAsync(
        int parentBlockId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentBlockRelation>> ListParentsAsync(
        int childBlockId,
        CancellationToken cancellationToken = default);
}

public interface IHandoutRepository : IRepository<Handout>
{
}

public interface IHandoutVersionRepository : IRepository<HandoutVersion>
{
    Task<IReadOnlyList<HandoutVersion>> ListByHandoutAsync(int handoutId, CancellationToken cancellationToken = default);
}

public interface IHandoutVersionItemRepository : IRepository<HandoutVersionItem>
{
    Task<IReadOnlyList<HandoutVersionItem>> ListByHandoutVersionAsync(
        int handoutVersionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HandoutVersionItem>> ListByTargetAsync(
        HandoutVersionItemTargetType targetType,
        int targetId,
        CancellationToken cancellationToken = default);
}

public interface IOutputTemplateRepository : IRepository<OutputTemplate>
{
}

public interface IOutputFormRepository : IRepository<OutputForm>
{
    Task<IReadOnlyList<OutputForm>> ListByHandoutVersionAsync(
        int handoutVersionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OutputForm>> ListByTemplateAsync(
        int outputTemplateId,
        CancellationToken cancellationToken = default);
}

public interface IGeneratedFileRepository : IRepository<GeneratedFile>
{
    Task<IReadOnlyList<GeneratedFile>> ListByOutputFormAsync(
        int outputFormId,
        CancellationToken cancellationToken = default);
}

public interface ITeachingNoteRepository : IRepository<TeachingNote>
{
    Task<IReadOnlyList<TeachingNote>> ListByTargetAsync(
        TeachingNoteTargetType targetType,
        int targetId,
        CancellationToken cancellationToken = default);
}
