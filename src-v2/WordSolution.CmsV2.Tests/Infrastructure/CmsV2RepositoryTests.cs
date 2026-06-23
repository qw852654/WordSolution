using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Repositories;
using WordSolution.CmsV2.Infrastructure.Persistence;
using WordSolution.CmsV2.Infrastructure.Repositories;

namespace WordSolution.CmsV2.Tests.Infrastructure;

public sealed class CmsV2RepositoryTests
{
    [Fact]
    public async Task UnitOfWork_can_write_and_read_entities_with_shared_context()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);

        var topic = new TeachingTopic("力学", sortOrder: 1);

        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        var loaded = await unitOfWork.TeachingTopics.GetByIdAsync(topic.Id);

        Assert.NotNull(loaded);
        Assert.Equal("力学", loaded.Name);
    }

    [Fact]
    public async Task Repository_properties_are_available_and_empty_lists_return_empty()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);

        Assert.Empty(await unitOfWork.TeachingTopics.ListAsync());
        Assert.Empty(await unitOfWork.Sections.ListAsync());
        Assert.Empty(await unitOfWork.SectionItems.ListAsync());
        Assert.Empty(await unitOfWork.AtomicSections.ListAsync());
        Assert.Empty(await unitOfWork.AtomicSectionPanels.ListAsync());
        Assert.Empty(await unitOfWork.AtomicSectionItems.ListAsync());
        Assert.Empty(await unitOfWork.SectionVariants.ListAsync());
        Assert.Empty(await unitOfWork.SectionVariantItems.ListAsync());
        Assert.Empty(await unitOfWork.ContentBlocks.ListAsync());
        Assert.Empty(await unitOfWork.ContentBlockVersions.ListAsync());
        Assert.Empty(await unitOfWork.ContentBlockRelations.ListAsync());
        Assert.Empty(await unitOfWork.Handouts.ListAsync());
        Assert.Empty(await unitOfWork.HandoutVersions.ListAsync());
        Assert.Empty(await unitOfWork.HandoutVersionItems.ListAsync());
        Assert.Empty(await unitOfWork.OutputTemplates.ListAsync());
        Assert.Empty(await unitOfWork.OutputForms.ListAsync());
        Assert.Empty(await unitOfWork.GeneratedFiles.ListAsync());
        Assert.Empty(await unitOfWork.TeachingNotes.ListAsync());
    }

    [Fact]
    public async Task Repositories_return_parent_child_and_polymorphic_queries_in_stable_order()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);

        var parentTopic = new TeachingTopic("功能关系");
        await unitOfWork.TeachingTopics.AddAsync(parentTopic);
        await unitOfWork.SaveChangesAsync();

        var childTopicB = new TeachingTopic("机械能守恒", parentId: parentTopic.Id, sortOrder: 2);
        var childTopicA = new TeachingTopic("动能定理", parentId: parentTopic.Id, sortOrder: 1);
        await unitOfWork.TeachingTopics.AddAsync(childTopicB);
        await unitOfWork.TeachingTopics.AddAsync(childTopicA);
        await unitOfWork.SaveChangesAsync();

        var sectionB = new Section(childTopicB.Id, "提高班", sortOrder: 2);
        var sectionA = new Section(childTopicA.Id, "基础班", sortOrder: 1);
        await unitOfWork.Sections.AddAsync(sectionB);
        await unitOfWork.Sections.AddAsync(sectionA);
        await unitOfWork.SaveChangesAsync();

        var atomicSection = new AtomicSection(sectionA.Id, "最小教学片段");
        var blockA = new ContentBlock(sectionA.Id, "知识点", ContentBlockType.KnowledgePoint);
        var blockB = new ContentBlock(sectionA.Id, "例题", ContentBlockType.Question, questionType: QuestionType.Calculation);
        var blockParent = new ContentBlock(sectionA.Id, "组合块", ContentBlockType.ExampleGroup);
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.ContentBlocks.AddAsync(blockA);
        await unitOfWork.ContentBlocks.AddAsync(blockB);
        await unitOfWork.ContentBlocks.AddAsync(blockParent);
        await unitOfWork.SaveChangesAsync();

        var panelB = new AtomicSectionPanel(
            atomicSection.Id,
            "渚嬮鏉垮潡",
            AtomicSectionTeachingRole.Example,
            Difficulty.Medium,
            sortOrder: 2);
        var panelA = new AtomicSectionPanel(
            atomicSection.Id,
            "Knowledge panel",
            AtomicSectionTeachingRole.Knowledge,
            Difficulty.Basic,
            sortOrder: 1);
        await unitOfWork.AtomicSectionPanels.AddAsync(panelB);
        await unitOfWork.AtomicSectionPanels.AddAsync(panelA);
        await unitOfWork.SaveChangesAsync();

        var blockVersion1 = new ContentBlockVersion(blockA.Id, 1, "content-blocks/source/a/v1.docx");
        var blockVersion2 = new ContentBlockVersion(blockA.Id, 2, "content-blocks/source/a/v2.docx", isCurrent: true);
        await unitOfWork.ContentBlockVersions.AddAsync(blockVersion1);
        await unitOfWork.ContentBlockVersions.AddAsync(blockVersion2);
        await unitOfWork.SaveChangesAsync();

        var sectionItemB = new SectionItem(
            sectionA.Id,
            SectionItemTargetType.ContentBlock,
            blockB.Id,
            ReferenceMode.FollowLatest,
            null,
            sortOrder: 2);
        var sectionItemA = new SectionItem(
            sectionA.Id,
            SectionItemTargetType.ContentBlock,
            blockA.Id,
            ReferenceMode.FollowLatest,
            null,
            sortOrder: 1);
        await unitOfWork.SectionItems.AddAsync(sectionItemB);
        await unitOfWork.SectionItems.AddAsync(sectionItemA);
        await unitOfWork.SaveChangesAsync();

        var atomicItem = new AtomicSectionItem(
            atomicSection.Id,
            blockA.Id,
            ReferenceMode.FollowLatest,
            null,
            sortOrder: 1,
            atomicSectionPanelId: panelA.Id,
            teachingRole: AtomicSectionTeachingRole.Knowledge);
        await unitOfWork.AtomicSectionItems.AddAsync(atomicItem);
        await unitOfWork.SaveChangesAsync();

        var sectionVariant = new SectionVariant(sectionA.Id, "课堂讲解版");
        await unitOfWork.SectionVariants.AddAsync(sectionVariant);
        await unitOfWork.SaveChangesAsync();

        var variantItemB = new SectionVariantItem(sectionVariant.Id, sectionItemB.Id, sortOrder: 2);
        var variantItemA = new SectionVariantItem(sectionVariant.Id, sectionItemA.Id, sortOrder: 1);
        await unitOfWork.SectionVariantItems.AddAsync(variantItemB);
        await unitOfWork.SectionVariantItems.AddAsync(variantItemA);
        await unitOfWork.SaveChangesAsync();

        var relationB = new ContentBlockRelation(
            blockParent.Id,
            blockB.Id,
            ReferenceMode.FollowLatest,
            null,
            sortOrder: 2);
        var relationA = new ContentBlockRelation(
            blockParent.Id,
            blockA.Id,
            ReferenceMode.FollowLatest,
            null,
            sortOrder: 1);
        await unitOfWork.ContentBlockRelations.AddAsync(relationB);
        await unitOfWork.ContentBlockRelations.AddAsync(relationA);
        await unitOfWork.SaveChangesAsync();

        var handout = new Handout("功能关系讲义");
        await unitOfWork.Handouts.AddAsync(handout);
        await unitOfWork.SaveChangesAsync();

        var handoutVersion = new HandoutVersion(handout.Id, "学生版", sortOrder: 1);
        await unitOfWork.HandoutVersions.AddAsync(handoutVersion);
        await unitOfWork.SaveChangesAsync();

        var handoutItemB = new HandoutVersionItem(
            handoutVersion.Id,
            HandoutVersionItemTargetType.ContentBlock,
            blockA.Id,
            sortOrder: 2);
        var handoutItemA = new HandoutVersionItem(
            handoutVersion.Id,
            HandoutVersionItemTargetType.SectionVariant,
            sectionVariant.Id,
            sortOrder: 1);
        await unitOfWork.HandoutVersionItems.AddAsync(handoutItemB);
        await unitOfWork.HandoutVersionItems.AddAsync(handoutItemA);
        await unitOfWork.SaveChangesAsync();

        var template = new OutputTemplate("默认模板", "templates/default.docx");
        await unitOfWork.OutputTemplates.AddAsync(template);
        await unitOfWork.SaveChangesAsync();

        var outputForm = new OutputForm(
            handoutVersion.Id,
            template.Id,
            "学生 Word",
            OutputAudience.Student,
            OutputFormat.Word,
            VisibilityMode.StudentNoAnswer,
            sortOrder: 1);
        await unitOfWork.OutputForms.AddAsync(outputForm);
        await unitOfWork.SaveChangesAsync();

        var generatedFile = new GeneratedFile(
            outputForm.Id,
            "handouts/generated/1/a.docx",
            "{}",
            DateTimeOffset.Parse("2026-06-09T12:00:00+08:00"));
        await unitOfWork.GeneratedFiles.AddAsync(generatedFile);

        var note = new TeachingNote(
            TeachingNoteTargetType.ContentBlock,
            blockA.Id,
            TeachingNoteType.TeachingLogic,
            "讲解提醒",
            "先讲守恒条件。");
        await unitOfWork.TeachingNotes.AddAsync(note);
        await unitOfWork.SaveChangesAsync();

        Assert.Equal(["动能定理", "机械能守恒"], (await unitOfWork.TeachingTopics.ListChildrenAsync(parentTopic.Id)).Select(x => x.Name));
        Assert.Equal(["基础班"], (await unitOfWork.Sections.ListByTeachingTopicAsync(childTopicA.Id)).Select(x => x.Title));
        Assert.Equal(["提高班"], (await unitOfWork.Sections.ListByTeachingTopicAsync(childTopicB.Id)).Select(x => x.Title));
        Assert.Equal([sectionItemA.Id, sectionItemB.Id], (await unitOfWork.SectionItems.ListBySectionAsync(sectionA.Id)).Select(x => x.Id));
        Assert.Equal([sectionItemA.Id], (await unitOfWork.SectionItems.ListByTargetAsync(SectionItemTargetType.ContentBlock, blockA.Id)).Select(x => x.Id));
        Assert.Equal([panelA.Id, panelB.Id], (await unitOfWork.AtomicSectionPanels.ListByAtomicSectionAsync(atomicSection.Id)).Select(x => x.Id));
        Assert.Equal([atomicItem.Id], (await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSection.Id)).Select(x => x.Id));
        Assert.Equal([atomicItem.Id], (await unitOfWork.AtomicSectionItems.ListByContentBlockAsync(blockA.Id)).Select(x => x.Id));
        Assert.Equal(panelA.Id, (await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSection.Id)).Single().AtomicSectionPanelId);
        Assert.Equal(AtomicSectionTeachingRole.Knowledge, (await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSection.Id)).Single().TeachingRole);
        Assert.Equal([sectionVariant.Id], (await unitOfWork.SectionVariants.ListBySectionAsync(sectionA.Id)).Select(x => x.Id));
        Assert.Equal([variantItemA.Id, variantItemB.Id], (await unitOfWork.SectionVariantItems.ListBySectionVariantAsync(sectionVariant.Id)).Select(x => x.Id));
        Assert.Equal([variantItemA.Id], (await unitOfWork.SectionVariantItems.ListBySectionItemAsync(sectionItemA.Id)).Select(x => x.Id));
        Assert.Equal([1, 2], (await unitOfWork.ContentBlockVersions.ListByContentBlockAsync(blockA.Id)).Select(x => x.VersionNumber));
        Assert.Equal(blockVersion2.Id, (await unitOfWork.ContentBlockVersions.GetByContentBlockAndVersionNumberAsync(blockA.Id, 2))?.Id);
        Assert.Equal(blockVersion2.Id, (await unitOfWork.ContentBlockVersions.GetCurrentByContentBlockAsync(blockA.Id))?.Id);
        Assert.Equal([relationA.Id, relationB.Id], (await unitOfWork.ContentBlockRelations.ListChildrenAsync(blockParent.Id)).Select(x => x.Id));
        Assert.Equal([relationA.Id], (await unitOfWork.ContentBlockRelations.ListParentsAsync(blockA.Id)).Select(x => x.Id));
        Assert.Equal([handoutVersion.Id], (await unitOfWork.HandoutVersions.ListByHandoutAsync(handout.Id)).Select(x => x.Id));
        Assert.Equal([handoutItemA.Id, handoutItemB.Id], (await unitOfWork.HandoutVersionItems.ListByHandoutVersionAsync(handoutVersion.Id)).Select(x => x.Id));
        Assert.Equal([handoutItemB.Id], (await unitOfWork.HandoutVersionItems.ListByTargetAsync(HandoutVersionItemTargetType.ContentBlock, blockA.Id)).Select(x => x.Id));
        Assert.Equal([outputForm.Id], (await unitOfWork.OutputForms.ListByHandoutVersionAsync(handoutVersion.Id)).Select(x => x.Id));
        Assert.Equal([outputForm.Id], (await unitOfWork.OutputForms.ListByTemplateAsync(template.Id)).Select(x => x.Id));
        Assert.Equal([generatedFile.Id], (await unitOfWork.GeneratedFiles.ListByOutputFormAsync(outputForm.Id)).Select(x => x.Id));
        Assert.Equal([note.Id], (await unitOfWork.TeachingNotes.ListByTargetAsync(TeachingNoteTargetType.ContentBlock, blockA.Id)).Select(x => x.Id));
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_commits_on_success_and_rolls_back_on_exception()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);

        await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
        {
            await unitOfWork.TeachingTopics.AddAsync(new TeachingTopic("提交成功"), cancellationToken);
        });

        Assert.Equal(["提交成功"], (await unitOfWork.TeachingTopics.ListAsync()).Select(x => x.Name));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                await unitOfWork.TeachingTopics.AddAsync(new TeachingTopic("应当回滚"), cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                throw new InvalidOperationException("rollback");
            }));

        Assert.Equal(["提交成功"], (await unitOfWork.TeachingTopics.ListAsync()).Select(x => x.Name));
    }

    [Fact]
    public async Task Delete_parent_with_child_reference_is_blocked_by_sqlite_restrict_constraint()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);

        var topic = new TeachingTopic("圆周运动");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        await unitOfWork.Sections.AddAsync(new Section(topic.Id, "竖直圆轨道"));
        await unitOfWork.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var savedTopic = await unitOfWork.TeachingTopics.GetByIdAsync(topic.Id);
        Assert.NotNull(savedTopic);

        unitOfWork.TeachingTopics.Remove(savedTopic);

        await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.SaveChangesAsync());
    }

    [Fact]
    public async Task Sections_teaching_topic_unique_constraint_blocks_duplicate_binding()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);

        var topic = new TeachingTopic("Unique topic");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        await unitOfWork.Sections.AddAsync(new Section(topic.Id, "First Section"));
        await unitOfWork.SaveChangesAsync();

        await unitOfWork.Sections.AddAsync(new Section(topic.Id, "Duplicate Section"));

        await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.SaveChangesAsync());
    }

    [Fact]
    public async Task Atomic_section_panel_unique_constraint_blocks_duplicate_role_and_difficulty()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);

        var topic = new TeachingTopic("Panel topic");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        var section = new Section(topic.Id, "Panel Section");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();

        var atomicSection = new AtomicSection(section.Id, "Panel AtomicSection");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();

        await unitOfWork.AtomicSectionPanels.AddAsync(new AtomicSectionPanel(
            atomicSection.Id,
            "First panel",
            AtomicSectionTeachingRole.Example,
            Difficulty.Basic,
            sortOrder: 1));
        await unitOfWork.SaveChangesAsync();

        await unitOfWork.AtomicSectionPanels.AddAsync(new AtomicSectionPanel(
            atomicSection.Id,
            "Duplicate panel",
            AtomicSectionTeachingRole.Example,
            Difficulty.Basic,
            sortOrder: 2));

        await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.SaveChangesAsync());
    }

    private static async Task<CmsV2DbContext> CreateMigratedContextAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-repository-tests",
            Guid.NewGuid().ToString("N"),
            "cms-v2.db");

        var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        return context;
    }
}
