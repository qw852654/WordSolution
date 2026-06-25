namespace WordSolution.CmsV2.Domain.Repositories;

public interface ICmsV2UnitOfWork
{
    ITeachingTopicRepository TeachingTopics { get; }

    ISectionRepository Sections { get; }

    ISectionItemRepository SectionItems { get; }

    IAtomicSectionRepository AtomicSections { get; }

    IAtomicSectionPanelRepository AtomicSectionPanels { get; }

    IAtomicSectionItemRepository AtomicSectionItems { get; }

    ISectionVariantRepository SectionVariants { get; }

    ISectionVariantItemRepository SectionVariantItems { get; }

    IContentBlockRepository ContentBlocks { get; }

    IContentBlockVersionRepository ContentBlockVersions { get; }

    IContentBlockVersionPartRepository ContentBlockVersionParts { get; }

    IContentBlockRelationRepository ContentBlockRelations { get; }

    IHandoutRepository Handouts { get; }

    IHandoutVersionRepository HandoutVersions { get; }

    IHandoutVersionItemRepository HandoutVersionItems { get; }

    IOutputTemplateRepository OutputTemplates { get; }

    IOutputFormRepository OutputForms { get; }

    IGeneratedFileRepository GeneratedFiles { get; }

    ITagRepository Tags { get; }

    ITagBindingRepository TagBindings { get; }

    ITeachingNoteRepository TeachingNotes { get; }

    ITeachingNoteBindingRepository TeachingNoteBindings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);

    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);
}
