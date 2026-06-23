using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Domain.Entities;

namespace WordSolution.CmsV2.Infrastructure.Persistence;

public sealed class CmsV2DbContext(DbContextOptions<CmsV2DbContext> options) : DbContext(options)
{
    public DbSet<TeachingTopic> TeachingTopics => Set<TeachingTopic>();

    public DbSet<Section> Sections => Set<Section>();

    public DbSet<SectionItem> SectionItems => Set<SectionItem>();

    public DbSet<AtomicSection> AtomicSections => Set<AtomicSection>();

    public DbSet<AtomicSectionPanel> AtomicSectionPanels => Set<AtomicSectionPanel>();

    public DbSet<AtomicSectionItem> AtomicSectionItems => Set<AtomicSectionItem>();

    public DbSet<SectionVariant> SectionVariants => Set<SectionVariant>();

    public DbSet<SectionVariantItem> SectionVariantItems => Set<SectionVariantItem>();

    public DbSet<ContentBlock> ContentBlocks => Set<ContentBlock>();

    public DbSet<ContentBlockVersion> ContentBlockVersions => Set<ContentBlockVersion>();

    public DbSet<ContentBlockRelation> ContentBlockRelations => Set<ContentBlockRelation>();

    public DbSet<Handout> Handouts => Set<Handout>();

    public DbSet<HandoutVersion> HandoutVersions => Set<HandoutVersion>();

    public DbSet<HandoutVersionItem> HandoutVersionItems => Set<HandoutVersionItem>();

    public DbSet<OutputTemplate> OutputTemplates => Set<OutputTemplate>();

    public DbSet<OutputForm> OutputForms => Set<OutputForm>();

    public DbSet<GeneratedFile> GeneratedFiles => Set<GeneratedFile>();

    public DbSet<TeachingNote> TeachingNotes => Set<TeachingNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CmsV2DbContext).Assembly);

        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(entityType => entityType.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
