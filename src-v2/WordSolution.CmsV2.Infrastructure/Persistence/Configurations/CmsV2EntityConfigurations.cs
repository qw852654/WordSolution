using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordSolution.CmsV2.Domain.Entities;

namespace WordSolution.CmsV2.Infrastructure.Persistence.Configurations;

internal static class CmsV2EntityConfiguration
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int NoteMaxLength = 1000;
    public const int PathMaxLength = 1024;

    public static void ConfigurePrimaryKey<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.HasKey("Id");
        builder.Property<int>("Id").ValueGeneratedOnAdd();
    }

    public static void ConfigureUpdatedTime<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.Property<DateTimeOffset>("UpdatedTime")
            .HasConversion<string>()
            .IsRequired();
    }
}

internal sealed class TeachingTopicConfiguration : IEntityTypeConfiguration<TeachingTopic>
{
    public void Configure(EntityTypeBuilder<TeachingTopic> builder)
    {
        builder.ToTable("TeachingTopics");
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.ParentId);
        builder.Property(entity => entity.Name).HasMaxLength(CmsV2EntityConfiguration.TitleMaxLength).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(CmsV2EntityConfiguration.DescriptionMaxLength);
        builder.Property(entity => entity.SortOrder).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<int>().IsRequired();
        CmsV2EntityConfiguration.ConfigureUpdatedTime(builder);

        builder.HasIndex(entity => entity.ParentId);
        builder.HasOne<TeachingTopic>()
            .WithMany()
            .HasForeignKey(entity => entity.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.ToTable("Sections");
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.TeachingTopicId).IsRequired();
        builder.Property(entity => entity.Title).HasMaxLength(CmsV2EntityConfiguration.TitleMaxLength).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(CmsV2EntityConfiguration.DescriptionMaxLength);
        builder.Property(entity => entity.Type).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.Difficulty).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.Status).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.SortOrder).IsRequired();
        CmsV2EntityConfiguration.ConfigureUpdatedTime(builder);

        builder.HasIndex(entity => new { entity.TeachingTopicId, entity.SortOrder });
        builder.HasOne<TeachingTopic>()
            .WithMany()
            .HasForeignKey(entity => entity.TeachingTopicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SectionItemConfiguration : IEntityTypeConfiguration<SectionItem>
{
    public void Configure(EntityTypeBuilder<SectionItem> builder)
    {
        builder.ToTable(
            "SectionItems",
            table =>
            {
                table.HasCheckConstraint("CK_SectionItems_TargetType", "\"TargetType\" IN (1, 2)");
                table.HasCheckConstraint(
                    "CK_SectionItems_ContentBlockReferenceMode",
                    "\"TargetType\" = 1 OR (\"ReferenceMode\" = 1 AND \"LockedContentBlockVersionId\" IS NULL)");
                table.HasCheckConstraint(
                    "CK_SectionItems_LockedVersionRequiresVersion",
                    "\"ReferenceMode\" <> 2 OR \"LockedContentBlockVersionId\" IS NOT NULL");
            });
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.SectionId).IsRequired();
        builder.Property(entity => entity.TargetType).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.TargetId).IsRequired();
        builder.Property(entity => entity.ReferenceMode).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.LockedContentBlockVersionId);
        builder.Property(entity => entity.TitleOverride).HasMaxLength(CmsV2EntityConfiguration.TitleMaxLength);
        builder.Property(entity => entity.ParentItemId);
        builder.Property(entity => entity.SortOrder).IsRequired();
        builder.Property(entity => entity.SelectionLayer);
        builder.Property(entity => entity.TeachingUseOverride);
        builder.Property(entity => entity.Status).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.Note).HasMaxLength(CmsV2EntityConfiguration.NoteMaxLength);
        CmsV2EntityConfiguration.ConfigureUpdatedTime(builder);

        builder.HasIndex(entity => new { entity.SectionId, entity.SortOrder });
        builder.HasIndex(entity => new { entity.TargetType, entity.TargetId });
        builder.HasIndex(entity => entity.ParentItemId);
        builder.HasIndex(entity => entity.LockedContentBlockVersionId);

        builder.HasOne<Section>()
            .WithMany()
            .HasForeignKey(entity => entity.SectionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SectionItem>()
            .WithMany()
            .HasForeignKey(entity => entity.ParentItemId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ContentBlockVersion>()
            .WithMany()
            .HasForeignKey(entity => entity.LockedContentBlockVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class AtomicSectionConfiguration : IEntityTypeConfiguration<AtomicSection>
{
    public void Configure(EntityTypeBuilder<AtomicSection> builder)
    {
        builder.ToTable("AtomicSections");
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.Title).HasMaxLength(CmsV2EntityConfiguration.TitleMaxLength).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(CmsV2EntityConfiguration.DescriptionMaxLength);
        builder.Property(entity => entity.Type).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.Status).HasConversion<int>().IsRequired();
        CmsV2EntityConfiguration.ConfigureUpdatedTime(builder);
    }
}

internal sealed class AtomicSectionItemConfiguration : IEntityTypeConfiguration<AtomicSectionItem>
{
    public void Configure(EntityTypeBuilder<AtomicSectionItem> builder)
    {
        builder.ToTable(
            "AtomicSectionItems",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_AtomicSectionItems_LockedVersionRequiresVersion",
                    "\"ReferenceMode\" <> 2 OR \"LockedContentBlockVersionId\" IS NOT NULL");
            });
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.AtomicSectionId).IsRequired();
        builder.Property(entity => entity.ContentBlockId).IsRequired();
        builder.Property(entity => entity.ReferenceMode).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.LockedContentBlockVersionId);
        builder.Property(entity => entity.TitleOverride).HasMaxLength(CmsV2EntityConfiguration.TitleMaxLength);
        builder.Property(entity => entity.SortOrder).IsRequired();
        builder.Property(entity => entity.Note).HasMaxLength(CmsV2EntityConfiguration.NoteMaxLength);
        CmsV2EntityConfiguration.ConfigureUpdatedTime(builder);

        builder.HasIndex(entity => new { entity.AtomicSectionId, entity.SortOrder });
        builder.HasIndex(entity => entity.ContentBlockId);
        builder.HasIndex(entity => entity.LockedContentBlockVersionId);

        builder.HasOne<AtomicSection>()
            .WithMany()
            .HasForeignKey(entity => entity.AtomicSectionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ContentBlock>()
            .WithMany()
            .HasForeignKey(entity => entity.ContentBlockId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ContentBlockVersion>()
            .WithMany()
            .HasForeignKey(entity => entity.LockedContentBlockVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SectionVariantConfiguration : IEntityTypeConfiguration<SectionVariant>
{
    public void Configure(EntityTypeBuilder<SectionVariant> builder)
    {
        builder.ToTable("SectionVariants");
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.SectionId).IsRequired();
        builder.Property(entity => entity.Title).HasMaxLength(CmsV2EntityConfiguration.TitleMaxLength).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(CmsV2EntityConfiguration.DescriptionMaxLength);
        builder.Property(entity => entity.Type).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.Difficulty).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.Status).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.SortOrder).IsRequired();
        CmsV2EntityConfiguration.ConfigureUpdatedTime(builder);

        builder.HasIndex(entity => new { entity.SectionId, entity.SortOrder });
        builder.HasOne<Section>()
            .WithMany()
            .HasForeignKey(entity => entity.SectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SectionVariantItemConfiguration : IEntityTypeConfiguration<SectionVariantItem>
{
    public void Configure(EntityTypeBuilder<SectionVariantItem> builder)
    {
        builder.ToTable("SectionVariantItems");
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.SectionVariantId).IsRequired();
        builder.Property(entity => entity.SectionItemId).IsRequired();
        builder.Property(entity => entity.SortOrder).IsRequired();
        builder.Property(entity => entity.Note).HasMaxLength(CmsV2EntityConfiguration.NoteMaxLength);
        CmsV2EntityConfiguration.ConfigureUpdatedTime(builder);

        builder.HasIndex(entity => new { entity.SectionVariantId, entity.SortOrder });
        builder.HasIndex(entity => new { entity.SectionVariantId, entity.SectionItemId }).IsUnique();

        builder.HasOne<SectionVariant>()
            .WithMany()
            .HasForeignKey(entity => entity.SectionVariantId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SectionItem>()
            .WithMany()
            .HasForeignKey(entity => entity.SectionItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ContentBlockConfiguration : IEntityTypeConfiguration<ContentBlock>
{
    public void Configure(EntityTypeBuilder<ContentBlock> builder)
    {
        builder.ToTable("ContentBlocks");
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.Title).HasMaxLength(CmsV2EntityConfiguration.TitleMaxLength).IsRequired();
        builder.Property(entity => entity.Summary).HasMaxLength(CmsV2EntityConfiguration.DescriptionMaxLength);
        builder.Property(entity => entity.BlockType).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.Difficulty).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.QuestionType);
        builder.Property(entity => entity.Status).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.CurrentVersionId);
        CmsV2EntityConfiguration.ConfigureUpdatedTime(builder);

        builder.HasIndex(entity => entity.CurrentVersionId);
        builder.HasOne<ContentBlockVersion>()
            .WithMany()
            .HasForeignKey(entity => entity.CurrentVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ContentBlockVersionConfiguration : IEntityTypeConfiguration<ContentBlockVersion>
{
    public void Configure(EntityTypeBuilder<ContentBlockVersion> builder)
    {
        builder.ToTable("ContentBlockVersions");
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.ContentBlockId).IsRequired();
        builder.Property(entity => entity.VersionNumber).IsRequired();
        builder.Property(entity => entity.DocxPath).HasMaxLength(CmsV2EntityConfiguration.PathMaxLength).IsRequired();
        builder.Property(entity => entity.HtmlPreviewPath).HasMaxLength(CmsV2EntityConfiguration.PathMaxLength);
        builder.Property(entity => entity.PlainText);
        builder.Property(entity => entity.IsCurrent).IsRequired();
        CmsV2EntityConfiguration.ConfigureUpdatedTime(builder);

        builder.HasIndex(entity => new { entity.ContentBlockId, entity.VersionNumber }).IsUnique();
        builder.HasIndex(entity => new { entity.ContentBlockId, entity.IsCurrent });
        builder.HasOne<ContentBlock>()
            .WithMany()
            .HasForeignKey(entity => entity.ContentBlockId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ContentBlockRelationConfiguration : IEntityTypeConfiguration<ContentBlockRelation>
{
    public void Configure(EntityTypeBuilder<ContentBlockRelation> builder)
    {
        builder.ToTable(
            "ContentBlockRelations",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_ContentBlockRelations_NoDirectSelfReference",
                    "\"ParentBlockId\" <> \"ChildBlockId\"");
                table.HasCheckConstraint(
                    "CK_ContentBlockRelations_LockedVersionRequiresVersion",
                    "\"ReferenceMode\" <> 2 OR \"LockedContentBlockVersionId\" IS NOT NULL");
            });
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.ParentBlockId).IsRequired();
        builder.Property(entity => entity.ChildBlockId).IsRequired();
        builder.Property(entity => entity.ReferenceMode).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.LockedContentBlockVersionId);
        builder.Property(entity => entity.TitleOverride).HasMaxLength(CmsV2EntityConfiguration.TitleMaxLength);
        builder.Property(entity => entity.SortOrder).IsRequired();
        builder.Property(entity => entity.Note).HasMaxLength(CmsV2EntityConfiguration.NoteMaxLength);
        CmsV2EntityConfiguration.ConfigureUpdatedTime(builder);

        builder.HasIndex(entity => new { entity.ParentBlockId, entity.SortOrder });
        builder.HasIndex(entity => entity.ChildBlockId);
        builder.HasIndex(entity => entity.LockedContentBlockVersionId);

        builder.HasOne<ContentBlock>()
            .WithMany()
            .HasForeignKey(entity => entity.ParentBlockId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ContentBlock>()
            .WithMany()
            .HasForeignKey(entity => entity.ChildBlockId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ContentBlockVersion>()
            .WithMany()
            .HasForeignKey(entity => entity.LockedContentBlockVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class HandoutConfiguration : IEntityTypeConfiguration<Handout>
{
    public void Configure(EntityTypeBuilder<Handout> builder)
    {
        builder.ToTable("Handouts");
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.Title).HasMaxLength(CmsV2EntityConfiguration.TitleMaxLength).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(CmsV2EntityConfiguration.DescriptionMaxLength);
        builder.Property(entity => entity.Status).HasConversion<int>().IsRequired();
        CmsV2EntityConfiguration.ConfigureUpdatedTime(builder);
    }
}

internal sealed class HandoutVersionConfiguration : IEntityTypeConfiguration<HandoutVersion>
{
    public void Configure(EntityTypeBuilder<HandoutVersion> builder)
    {
        builder.ToTable("HandoutVersions");
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.HandoutId).IsRequired();
        builder.Property(entity => entity.Title).HasMaxLength(CmsV2EntityConfiguration.TitleMaxLength).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(CmsV2EntityConfiguration.DescriptionMaxLength);
        builder.Property(entity => entity.Type).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.Status).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.SortOrder).IsRequired();
        CmsV2EntityConfiguration.ConfigureUpdatedTime(builder);

        builder.HasIndex(entity => new { entity.HandoutId, entity.SortOrder });
        builder.HasOne<Handout>()
            .WithMany()
            .HasForeignKey(entity => entity.HandoutId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class HandoutVersionItemConfiguration : IEntityTypeConfiguration<HandoutVersionItem>
{
    public void Configure(EntityTypeBuilder<HandoutVersionItem> builder)
    {
        builder.ToTable(
            "HandoutVersionItems",
            table => table.HasCheckConstraint("CK_HandoutVersionItems_TargetType", "\"TargetType\" IN (1, 2)"));
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.HandoutVersionId).IsRequired();
        builder.Property(entity => entity.TargetType).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.TargetId).IsRequired();
        builder.Property(entity => entity.SortOrder).IsRequired();
        builder.Property(entity => entity.TitleOverride).HasMaxLength(CmsV2EntityConfiguration.TitleMaxLength);
        builder.Property(entity => entity.Note).HasMaxLength(CmsV2EntityConfiguration.NoteMaxLength);
        CmsV2EntityConfiguration.ConfigureUpdatedTime(builder);

        builder.HasIndex(entity => new { entity.HandoutVersionId, entity.SortOrder });
        builder.HasIndex(entity => new { entity.TargetType, entity.TargetId });
        builder.HasOne<HandoutVersion>()
            .WithMany()
            .HasForeignKey(entity => entity.HandoutVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class OutputTemplateConfiguration : IEntityTypeConfiguration<OutputTemplate>
{
    public void Configure(EntityTypeBuilder<OutputTemplate> builder)
    {
        builder.ToTable("OutputTemplates");
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.Title).HasMaxLength(CmsV2EntityConfiguration.TitleMaxLength).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(CmsV2EntityConfiguration.DescriptionMaxLength);
        builder.Property(entity => entity.TemplateDocxPath).HasMaxLength(CmsV2EntityConfiguration.PathMaxLength).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<int>().IsRequired();
        CmsV2EntityConfiguration.ConfigureUpdatedTime(builder);
    }
}

internal sealed class OutputFormConfiguration : IEntityTypeConfiguration<OutputForm>
{
    public void Configure(EntityTypeBuilder<OutputForm> builder)
    {
        builder.ToTable("OutputForms");
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.HandoutVersionId).IsRequired();
        builder.Property(entity => entity.OutputTemplateId).IsRequired();
        builder.Property(entity => entity.Title).HasMaxLength(CmsV2EntityConfiguration.TitleMaxLength).IsRequired();
        builder.Property(entity => entity.Audience).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.OutputFormat).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.VisibilityMode).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.Status).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.SortOrder).IsRequired();
        CmsV2EntityConfiguration.ConfigureUpdatedTime(builder);

        builder.HasIndex(entity => new { entity.HandoutVersionId, entity.SortOrder });
        builder.HasIndex(entity => entity.OutputTemplateId);

        builder.HasOne<HandoutVersion>()
            .WithMany()
            .HasForeignKey(entity => entity.HandoutVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<OutputTemplate>()
            .WithMany()
            .HasForeignKey(entity => entity.OutputTemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class GeneratedFileConfiguration : IEntityTypeConfiguration<GeneratedFile>
{
    public void Configure(EntityTypeBuilder<GeneratedFile> builder)
    {
        builder.ToTable("GeneratedFiles");
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.OutputFormId).IsRequired();
        builder.Property(entity => entity.FilePath).HasMaxLength(CmsV2EntityConfiguration.PathMaxLength).IsRequired();
        builder.Property(entity => entity.VersionManifestJson).IsRequired();
        builder.Property(entity => entity.GeneratedTime).HasConversion<string>().IsRequired();

        builder.HasIndex(entity => entity.OutputFormId);
        builder.HasOne<OutputForm>()
            .WithMany()
            .HasForeignKey(entity => entity.OutputFormId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class TeachingNoteConfiguration : IEntityTypeConfiguration<TeachingNote>
{
    public void Configure(EntityTypeBuilder<TeachingNote> builder)
    {
        builder.ToTable("TeachingNotes");
        CmsV2EntityConfiguration.ConfigurePrimaryKey(builder);

        builder.Property(entity => entity.TargetType).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.TargetId).IsRequired();
        builder.Property(entity => entity.NoteType).HasConversion<int>().IsRequired();
        builder.Property(entity => entity.Title).HasMaxLength(CmsV2EntityConfiguration.TitleMaxLength).IsRequired();
        builder.Property(entity => entity.Content).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<int>().IsRequired();
        CmsV2EntityConfiguration.ConfigureUpdatedTime(builder);

        builder.HasIndex(entity => new { entity.TargetType, entity.TargetId });
    }
}
