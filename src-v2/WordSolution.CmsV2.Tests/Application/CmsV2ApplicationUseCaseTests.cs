using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Application.AtomicSections;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Application.ContentBlocks;
using WordSolution.CmsV2.Application.Handouts;
using WordSolution.CmsV2.Application.Sections;
using WordSolution.CmsV2.Application.SectionVariants;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Infrastructure.Persistence;
using WordSolution.CmsV2.Infrastructure.Repositories;

namespace WordSolution.CmsV2.Tests.Application;

public sealed class CmsV2ApplicationUseCaseTests
{
    [Fact]
    public async Task ContentBlock_use_cases_create_versions_and_switch_current_version()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var useCases = new ContentBlockUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var created = await useCases.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(
                sectionId,
                "动能定理",
                ContentBlockType.KnowledgePoint,
                "content-blocks/source/1/v1.docx",
                PlainText: "动能变化等于合外力做功"));

        var createdVersion = await useCases.CreateContentBlockVersionAsync(
            new CreateContentBlockVersionCommand(
                created.Id,
                "content-blocks/source/1/v2.docx",
                PlainText: "动能定理第二版",
                SetAsCurrent: false));

        await useCases.SetCurrentContentBlockVersionAsync(
            new SetCurrentContentBlockVersionCommand(created.Id, createdVersion.Id));

        var block = await unitOfWork.ContentBlocks.GetByIdAsync(created.Id);
        var versions = await unitOfWork.ContentBlockVersions.ListByContentBlockAsync(created.Id);

        Assert.NotNull(block);
        Assert.Equal(createdVersion.Id, block.CurrentVersionId);
        Assert.Equal([1, 2], versions.Select(version => version.VersionNumber));
        Assert.False(versions.Single(version => version.VersionNumber == 1).IsCurrent);
        Assert.True(versions.Single(version => version.VersionNumber == 2).IsCurrent);
    }

    [Fact]
    public async Task SetCurrentContentBlockVersion_rejects_version_from_another_content_block()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var useCases = new ContentBlockUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var first = await useCases.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(
                sectionId,
                "内容块 A",
                ContentBlockType.Explanation,
                "content-blocks/source/a/v1.docx"));
        var second = await useCases.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(
                sectionId,
                "内容块 B",
                ContentBlockType.Explanation,
                "content-blocks/source/b/v1.docx"));
        var secondVersion = await unitOfWork.ContentBlockVersions.GetCurrentByContentBlockAsync(second.Id);

        Assert.NotNull(secondVersion);

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => useCases.SetCurrentContentBlockVersionAsync(
                new SetCurrentContentBlockVersionCommand(first.Id, secondVersion.Id)));
    }

    [Fact]
    public async Task CreateContentBlockWithInitialVersion_rolls_back_when_version_creation_fails()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var useCases = new ContentBlockUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        await Assert.ThrowsAnyAsync<Exception>(
            () => useCases.CreateContentBlockWithInitialVersionAsync(
                new CreateContentBlockWithInitialVersionCommand(
                    sectionId,
                    "不应留下半成品",
                    ContentBlockType.GeneralText,
                    " ")));

        Assert.Empty(await unitOfWork.ContentBlocks.ListAsync());
        Assert.Empty(await unitOfWork.ContentBlockVersions.ListAsync());
    }

    [Fact]
    public async Task ContentBlockRelation_use_case_validates_locked_version_and_recursive_cycles()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var relations = new ContentBlockRelationUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var parent = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "父组合块", ContentBlockType.ExampleGroup, "parent/v1.docx"));
        var child = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "子块", ContentBlockType.KnowledgePoint, "child/v1.docx"));
        var grandChild = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "孙块", ContentBlockType.Question, "grand-child/v1.docx"));
        var parentVersion = await unitOfWork.ContentBlockVersions.GetCurrentByContentBlockAsync(parent.Id);

        Assert.NotNull(parentVersion);

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => relations.AddContentBlockRelationAsync(
                new AddContentBlockRelationCommand(
                    parent.Id,
                    child.Id,
                    ReferenceMode.LockedVersion,
                    parentVersion.Id,
                    SortOrder: 1)));

        await relations.AddContentBlockRelationAsync(
            new AddContentBlockRelationCommand(parent.Id, child.Id, ReferenceMode.FollowLatest, null, SortOrder: 1));
        await relations.AddContentBlockRelationAsync(
            new AddContentBlockRelationCommand(child.Id, grandChild.Id, ReferenceMode.FollowLatest, null, SortOrder: 1));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => relations.AddContentBlockRelationAsync(
                new AddContentBlockRelationCommand(grandChild.Id, parent.Id, ReferenceMode.FollowLatest, null, SortOrder: 1)));
        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => relations.AddContentBlockRelationAsync(
                new AddContentBlockRelationCommand(parent.Id, parent.Id, ReferenceMode.FollowLatest, null, SortOrder: 2)));
    }

    [Fact]
    public async Task ContentBlockRelation_use_case_can_move_and_remove_child_relations()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var relations = new ContentBlockRelationUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var parent = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "parent", ContentBlockType.ExampleGroup, "parent/v1.docx"));
        var firstChild = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "first", ContentBlockType.Question, "first/v1.docx"));
        var secondChild = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "second", ContentBlockType.Question, "second/v1.docx"));

        var firstRelation = await relations.AddContentBlockRelationAsync(
            new AddContentBlockRelationCommand(parent.Id, firstChild.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        var secondRelation = await relations.AddContentBlockRelationAsync(
            new AddContentBlockRelationCommand(parent.Id, secondChild.Id, ReferenceMode.FollowLatest, null, SortOrder: 20));

        await relations.MoveContentBlockRelationAsync(
            new MoveContentBlockRelationCommand(parent.Id, secondRelation.Id, ContentBlockRelationMoveDirection.Up));

        var movedChildren = await unitOfWork.ContentBlockRelations.ListChildrenAsync(parent.Id);
        Assert.Equal([secondRelation.Id, firstRelation.Id], movedChildren.Select(relation => relation.Id));

        await relations.RemoveContentBlockRelationAsync(
            new RemoveContentBlockRelationCommand(parent.Id, secondRelation.Id));

        var remainingChildren = await unitOfWork.ContentBlockRelations.ListChildrenAsync(parent.Id);
        Assert.Equal([firstRelation.Id], remainingChildren.Select(relation => relation.Id));
        Assert.Null(await unitOfWork.ContentBlockRelations.GetByIdAsync(secondRelation.Id));
    }

    [Fact]
    public async Task AtomicSection_use_case_can_move_and_remove_child_items()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var atomicSections = new AtomicSectionUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var atomicSection = new AtomicSection(sectionId, "atomic");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();

        var firstChild = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "first", ContentBlockType.Question, "atomic-first/v1.docx"));
        var secondChild = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "second", ContentBlockType.Question, "atomic-second/v1.docx"));

        var firstItem = await atomicSections.AddAtomicSectionItemAsync(
            new AddAtomicSectionItemCommand(atomicSection.Id, firstChild.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        var secondItem = await atomicSections.AddAtomicSectionItemAsync(
            new AddAtomicSectionItemCommand(atomicSection.Id, secondChild.Id, ReferenceMode.FollowLatest, null, SortOrder: 20));

        await atomicSections.MoveAtomicSectionItemAsync(
            new MoveAtomicSectionItemCommand(atomicSection.Id, secondItem.Id, AtomicSectionItemMoveDirection.Up));

        var movedItems = await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSection.Id);
        Assert.Equal([secondItem.Id, firstItem.Id], movedItems.Select(item => item.Id));

        await atomicSections.RemoveAtomicSectionItemAsync(
            new RemoveAtomicSectionItemCommand(atomicSection.Id, secondItem.Id));

        var remainingItems = await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSection.Id);
        Assert.Equal([firstItem.Id], remainingItems.Select(item => item.Id));
        Assert.Null(await unitOfWork.AtomicSectionItems.GetByIdAsync(secondItem.Id));
        Assert.NotNull(await unitOfWork.ContentBlocks.GetByIdAsync(secondChild.Id));
    }

    [Fact]
    public async Task Section_and_atomic_section_use_cases_validate_targets_and_locked_versions()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sectionUseCases = new SectionUseCases(unitOfWork);
        var atomicUseCases = new AtomicSectionUseCases(unitOfWork);

        var topic = new TeachingTopic("机械能");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        var section = new Section(topic.Id, "机械能守恒");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();

        var atomicSection = new AtomicSection(section.Id, "守恒条件");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();

        var block = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(section.Id, "守恒条件内容", ContentBlockType.KnowledgePoint, "block/v1.docx"));
        var otherBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(section.Id, "其他内容", ContentBlockType.KnowledgePoint, "other/v1.docx"));
        var otherVersion = await unitOfWork.ContentBlockVersions.GetCurrentByContentBlockAsync(otherBlock.Id);

        Assert.NotNull(otherVersion);

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => sectionUseCases.AddSectionItemAsync(
                new AddSectionItemCommand(
                    section.Id,
                    SectionItemTargetType.ContentBlock,
                    block.Id,
                    ReferenceMode.LockedVersion,
                    otherVersion.Id,
                    SortOrder: 1)));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => sectionUseCases.AddSectionItemAsync(
                new AddSectionItemCommand(
                    section.Id,
                    SectionItemTargetType.AtomicSection,
                    atomicSection.Id,
                    ReferenceMode.LockedVersion,
                    otherVersion.Id,
                    SortOrder: 1)));

        var sectionItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(
                section.Id,
                SectionItemTargetType.ContentBlock,
                block.Id,
                ReferenceMode.FollowLatest,
                null,
                SortOrder: 1));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => atomicUseCases.AddAtomicSectionItemAsync(
                new AddAtomicSectionItemCommand(
                    atomicSection.Id,
                    block.Id,
                    ReferenceMode.LockedVersion,
                    otherVersion.Id,
                    SortOrder: 1)));

        var atomicItem = await atomicUseCases.AddAtomicSectionItemAsync(
            new AddAtomicSectionItemCommand(
                atomicSection.Id,
                block.Id,
                ReferenceMode.FollowLatest,
                null,
                SortOrder: 1));

        Assert.True(sectionItem.Id > 0);
        Assert.True(atomicItem.Id > 0);
    }

    [Fact]
    public async Task SectionVariant_use_cases_require_items_from_the_same_section()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sectionUseCases = new SectionUseCases(unitOfWork);
        var variantUseCases = new SectionVariantUseCases(unitOfWork);

        var topic = new TeachingTopic("圆周运动");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        var sectionA = new Section(topic.Id, "竖直圆轨道");
        var sectionB = new Section(topic.Id, "水平圆周");
        await unitOfWork.Sections.AddAsync(sectionA);
        await unitOfWork.Sections.AddAsync(sectionB);
        await unitOfWork.SaveChangesAsync();

        var block = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionA.Id, "例题", ContentBlockType.Question, "question/v1.docx"));
        var itemA = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionA.Id, SectionItemTargetType.ContentBlock, block.Id, ReferenceMode.FollowLatest, null, SortOrder: 1));
        var itemB = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionB.Id, SectionItemTargetType.ContentBlock, block.Id, ReferenceMode.FollowLatest, null, SortOrder: 1));

        var variant = await variantUseCases.CreateSectionVariantAsync(
            new CreateSectionVariantCommand(sectionA.Id, "课堂版"));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => variantUseCases.AddSectionVariantItemAsync(
                new AddSectionVariantItemCommand(variant.Id, itemB.Id, SortOrder: 1)));

        var variantItem = await variantUseCases.AddSectionVariantItemAsync(
            new AddSectionVariantItemCommand(variant.Id, itemA.Id, SortOrder: 1));

        Assert.True(variantItem.Id > 0);
    }

    [Fact]
    public async Task Handout_use_cases_create_versions_and_reject_section_or_atomic_section_targets()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var handouts = new HandoutUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var handout = new Handout("机械能讲义");
        await unitOfWork.Handouts.AddAsync(handout);
        await unitOfWork.SaveChangesAsync();

        var handoutVersion = await handouts.CreateHandoutVersionAsync(
            new CreateHandoutVersionCommand(handout.Id, "基础班"));
        var block = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "知识点", ContentBlockType.KnowledgePoint, "knowledge/v1.docx"));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => handouts.AddHandoutVersionItemAsync(
                new AddHandoutVersionItemCommand(
                    handoutVersion.Id,
                    HandoutVersionItemTargetType.Section,
                    TargetId: 1,
                    SortOrder: 1)));
        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => handouts.AddHandoutVersionItemAsync(
                new AddHandoutVersionItemCommand(
                    handoutVersion.Id,
                    HandoutVersionItemTargetType.AtomicSection,
                    TargetId: 1,
                    SortOrder: 1)));

        var item = await handouts.AddHandoutVersionItemAsync(
            new AddHandoutVersionItemCommand(
                handoutVersion.Id,
                HandoutVersionItemTargetType.ContentBlock,
                block.Id,
                SortOrder: 1));

        Assert.True(item.Id > 0);
    }

    private static async Task<CmsV2DbContext> CreateMigratedContextAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-application-tests",
            Guid.NewGuid().ToString("N"),
            "cms-v2.db");

        var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        return context;
    }

    private static async Task<int> CreateSectionAsync(EfCmsV2UnitOfWork unitOfWork)
    {
        var topic = new TeachingTopic("默认主题");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        var section = new Section(topic.Id, "默认 Section");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();

        return section.Id;
    }
}
