using WordSolution.CmsV2.Domain.Repositories;
using WordSolution.CmsV2.Infrastructure.Persistence;

namespace WordSolution.CmsV2.Infrastructure.Repositories;

public sealed class EfCmsV2UnitOfWork : ICmsV2UnitOfWork
{
    private readonly CmsV2DbContext _context;

    public EfCmsV2UnitOfWork(CmsV2DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));

        TeachingTopics = new EfTeachingTopicRepository(context);
        Sections = new EfSectionRepository(context);
        SectionItems = new EfSectionItemRepository(context);
        AtomicSections = new EfAtomicSectionRepository(context);
        AtomicSectionItems = new EfAtomicSectionItemRepository(context);
        SectionVariants = new EfSectionVariantRepository(context);
        SectionVariantItems = new EfSectionVariantItemRepository(context);
        ContentBlocks = new EfContentBlockRepository(context);
        ContentBlockVersions = new EfContentBlockVersionRepository(context);
        ContentBlockRelations = new EfContentBlockRelationRepository(context);
        Handouts = new EfHandoutRepository(context);
        HandoutVersions = new EfHandoutVersionRepository(context);
        HandoutVersionItems = new EfHandoutVersionItemRepository(context);
        OutputTemplates = new EfOutputTemplateRepository(context);
        OutputForms = new EfOutputFormRepository(context);
        GeneratedFiles = new EfGeneratedFileRepository(context);
        TeachingNotes = new EfTeachingNoteRepository(context);
    }

    public ITeachingTopicRepository TeachingTopics { get; }

    public ISectionRepository Sections { get; }

    public ISectionItemRepository SectionItems { get; }

    public IAtomicSectionRepository AtomicSections { get; }

    public IAtomicSectionItemRepository AtomicSectionItems { get; }

    public ISectionVariantRepository SectionVariants { get; }

    public ISectionVariantItemRepository SectionVariantItems { get; }

    public IContentBlockRepository ContentBlocks { get; }

    public IContentBlockVersionRepository ContentBlockVersions { get; }

    public IContentBlockRelationRepository ContentBlockRelations { get; }

    public IHandoutRepository Handouts { get; }

    public IHandoutVersionRepository HandoutVersions { get; }

    public IHandoutVersionItemRepository HandoutVersionItems { get; }

    public IOutputTemplateRepository OutputTemplates { get; }

    public IOutputFormRepository OutputForms { get; }

    public IGeneratedFileRepository GeneratedFiles { get; }

    public ITeachingNoteRepository TeachingNotes { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        await operation(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var result = await operation(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return result;
    }
}
