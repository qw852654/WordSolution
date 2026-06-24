using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Application.Handouts;
using WordSolution.CmsV2.Application.SectionVariants;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Infrastructure.Persistence;
using WordSolution.CmsV2.Infrastructure.Repositories;

namespace WordSolution.CmsV2.Tests.Application;

public sealed class CmsV2HandoutWorkspaceUseCaseTests
{
    [Fact]
    public async Task GetHandoutVersionWorkspaceAsync_returns_version_items_expanded_sources_and_output_history()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var handouts = new HandoutUseCases(unitOfWork);
        var variants = new SectionVariantUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var handout = new Handout("Handout");
        await unitOfWork.Handouts.AddAsync(handout);
        await unitOfWork.SaveChangesAsync();
        var handoutVersion = await handouts.CreateHandoutVersionAsync(
            new CreateHandoutVersionCommand(handout.Id, "Student version"));
        var defaultOutputForm = Assert.Single(await unitOfWork.OutputForms.ListByHandoutVersionAsync(handoutVersion.Id));

        var sectionBlock = await CreateContentBlockAsync(unitOfWork, sectionId, "Section block");
        var atomicBlock = await CreateContentBlockAsync(unitOfWork, sectionId, "Atomic child");
        var directBlock = await CreateContentBlockAsync(unitOfWork, sectionId, "Direct block");
        var sectionItem = new SectionItem(
            sectionId,
            SectionItemTargetType.ContentBlock,
            sectionBlock.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 10);
        await unitOfWork.SectionItems.AddAsync(sectionItem);
        await unitOfWork.SaveChangesAsync();

        var sectionVariant = await variants.CreateSectionVariantAsync(
            new CreateSectionVariantCommand(
                sectionId,
                "Variant",
                Difficulty: Difficulty.Basic,
                SelectedSectionItemIds: [sectionItem.Id]));

        var atomicSection = new AtomicSection(sectionId, "Atomic", difficulty: Difficulty.Medium);
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();
        await unitOfWork.AtomicSectionItems.AddAsync(
            new AtomicSectionItem(
                atomicSection.Id,
                atomicBlock.Id,
                ReferenceMode.FollowLatest,
                lockedContentBlockVersionId: null,
                sortOrder: 10));
        await unitOfWork.SaveChangesAsync();

        var variantItem = await handouts.AddHandoutVersionItemAsync(
            new AddHandoutVersionItemCommand(
                handoutVersion.Id,
                HandoutVersionItemTargetType.SectionVariant,
                sectionVariant.Id));
        var atomicItem = await handouts.AddHandoutVersionItemAsync(
            new AddHandoutVersionItemCommand(
                handoutVersion.Id,
                HandoutVersionItemTargetType.AtomicSection,
                atomicSection.Id));
        var directItem = await handouts.AddHandoutVersionItemAsync(
            new AddHandoutVersionItemCommand(
                handoutVersion.Id,
                HandoutVersionItemTargetType.ContentBlock,
                directBlock.Id));

        var template = new OutputTemplate("Template", "templates/default.docx");
        await unitOfWork.OutputTemplates.AddAsync(template);
        await unitOfWork.SaveChangesAsync();
        var outputForm = new OutputForm(
            handoutVersion.Id,
            template.Id,
            "Student Word",
            OutputAudience.Student,
            OutputFormat.Word,
            VisibilityMode.StudentNoAnswer,
            sortOrder: 10);
        await unitOfWork.OutputForms.AddAsync(outputForm);
        await unitOfWork.SaveChangesAsync();
        var generatedFile = new GeneratedFile(
            outputForm.Id,
            "handouts/generated/1/file.docx",
            "{}",
            DateTimeOffset.Parse("2026-06-22T08:00:00+08:00"));
        await unitOfWork.GeneratedFiles.AddAsync(generatedFile);
        await unitOfWork.SaveChangesAsync();

        var workspace = await handouts.GetHandoutVersionWorkspaceAsync(
            new GetHandoutVersionWorkspaceCommand(handoutVersion.Id));

        Assert.Equal(handout.Id, workspace.Handout.Id);
        Assert.Equal(handoutVersion.Id, workspace.Version.Id);
        Assert.Equal([variantItem.Id, atomicItem.Id, directItem.Id], workspace.Items.Select(item => item.HandoutVersionItemId));
        Assert.Equal(["SectionVariant", "AtomicSection", "ContentBlock"], workspace.Items.Select(item => item.TargetType));
        Assert.Equal("Variant", workspace.Items[0].Title);
        Assert.Contains(workspace.Items[0].Children, child => child.NodeKind == "SectionItem" && child.Title == "Section block");
        Assert.Contains(workspace.Items[1].Children, child => child.NodeKind == "AtomicSectionItem" && child.Children.Any(grandChild => grandChild.Title == "Atomic child"));
        Assert.Equal("Direct block", workspace.Items[2].Title);
        Assert.Equal([defaultOutputForm.Id, outputForm.Id], workspace.OutputForms.Select(form => form.Id));
        Assert.Equal([generatedFile.Id], workspace.GeneratedFiles.Select(file => file.Id));
    }

    private static async Task<int> CreateSectionAsync(EfCmsV2UnitOfWork unitOfWork)
    {
        var topic = new TeachingTopic("Topic");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        var section = new Section(topic.Id, "Section");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();

        return section.Id;
    }

    private static async Task<ContentBlock> CreateContentBlockAsync(
        EfCmsV2UnitOfWork unitOfWork,
        int sectionId,
        string title)
    {
        var block = new ContentBlock(sectionId, title, ContentBlockType.KnowledgePoint);
        await unitOfWork.ContentBlocks.AddAsync(block);
        await unitOfWork.SaveChangesAsync();

        return block;
    }

    private static async Task<CmsV2DbContext> CreateMigratedContextAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-handout-workspace-tests",
            Guid.NewGuid().ToString("N"),
            "cms-v2.db");

        var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        return context;
    }
}
