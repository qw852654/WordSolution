using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Application.AtomicSections;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Application.ContentBlocks;
using WordSolution.CmsV2.Application.Handouts;
using WordSolution.CmsV2.Application.Sections;
using WordSolution.CmsV2.Application.SectionVariants;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Infrastructure.Documents;
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
    public async Task CreateContentBlock_creates_metadata_without_docx_version()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var useCases = new ContentBlockUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var created = await useCases.CreateContentBlockAsync(
            new CreateContentBlockCommand(
                sectionId,
                string.Empty,
                ContentBlockType.KnowledgePoint,
                Difficulty.Basic));
        var block = await unitOfWork.ContentBlocks.GetByIdAsync(created.Id);
        var versions = await unitOfWork.ContentBlockVersions.ListByContentBlockAsync(created.Id);

        Assert.NotNull(block);
        Assert.Equal(sectionId, block.SectionId);
        Assert.Equal(string.Empty, block.Title);
        Assert.Equal(ContentBlockType.KnowledgePoint, block.BlockType);
        Assert.Equal(Difficulty.Basic, block.Difficulty);
        Assert.Null(block.CurrentVersionId);
        Assert.Empty(versions);
    }

    [Fact]
    public async Task DeleteContentBlockCascadeAsync_removes_block_assets_and_all_references()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sections = new SectionUseCases(unitOfWork);
        var variants = new SectionVariantUseCases(unitOfWork);
        var relations = new ContentBlockRelationUseCases(unitOfWork);
        var handouts = new HandoutUseCases(unitOfWork);
        var deletion = new ContentBlockDeletionUseCases(
            unitOfWork,
            new LocalContentBlockFileStore(),
            new FakeContentBlockEditSessionStore());
        var bankRootDirectory = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-delete-cascade-tests",
            Guid.NewGuid().ToString("N"));
        var sectionId = await CreateSectionAsync(unitOfWork);

        var target = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "例题组", ContentBlockType.ExampleGroup, Difficulty.Medium));
        var child = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "子题", ContentBlockType.Question, Difficulty.Basic));
        var parent = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "父组合", ContentBlockType.ExerciseGroup, Difficulty.Basic));
        var docxPath = Path.Combine(bankRootDirectory, "content-blocks", "source", target.Id.ToString(), "v1.docx");
        var htmlPath = Path.Combine(bankRootDirectory, "content-blocks", "html", target.Id.ToString(), "v1.html");
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(htmlPath)!);
        await File.WriteAllTextAsync(docxPath, "docx placeholder");
        await File.WriteAllTextAsync(htmlPath, "<html></html>");
        var version = await contentBlocks.CreateContentBlockVersionAsync(
            new CreateContentBlockVersionCommand(
                target.Id,
                docxPath,
                htmlPath,
                PlainText: "plain text",
                SetAsCurrent: true));
        var sectionItem = await sections.AddSectionItemAsync(
            new AddSectionItemCommand(
                sectionId,
                SectionItemTargetType.ContentBlock,
                target.Id,
                ReferenceMode.FollowLatest,
                null,
                SortOrder: 10));
        var variant = await variants.CreateSectionVariantAsync(
            new CreateSectionVariantCommand(
                sectionId,
                "基础版",
                Difficulty: Difficulty.Medium,
                SelectedSectionItemIds: [sectionItem.Id]));
        var atomicSection = new AtomicSection(sectionId, "原子小节", difficulty: Difficulty.Basic);
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();
        var atomicItem = new AtomicSectionItem(
            atomicSection.Id,
            target.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 10);
        await unitOfWork.AtomicSectionItems.AddAsync(atomicItem);
        await unitOfWork.SaveChangesAsync();
        await relations.AddContentBlockRelationAsync(
            new AddContentBlockRelationCommand(parent.Id, target.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        await relations.AddContentBlockRelationAsync(
            new AddContentBlockRelationCommand(target.Id, child.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        var handout = new Handout("讲义");
        await unitOfWork.Handouts.AddAsync(handout);
        await unitOfWork.SaveChangesAsync();
        var handoutVersion = await handouts.CreateHandoutVersionAsync(
            new CreateHandoutVersionCommand(handout.Id, "讲义版本"));
        await handouts.AddHandoutVersionItemAsync(
            new AddHandoutVersionItemCommand(
                handoutVersion.Id,
                HandoutVersionItemTargetType.ContentBlock,
                target.Id,
                SortOrder: 10));

        var result = await deletion.DeleteContentBlockCascadeAsync(
            new DeleteContentBlockCascadeCommand(bankRootDirectory, target.Id));

        Assert.Equal(target.Id, result.ContentBlockId);
        Assert.Equal(1, result.RemovedSectionItemCount);
        Assert.Equal(1, result.RemovedSectionVariantItemCount);
        Assert.Equal(1, result.RemovedAtomicSectionItemCount);
        Assert.Equal(2, result.RemovedContentBlockRelationCount);
        Assert.Equal(1, result.RemovedHandoutVersionItemCount);
        Assert.Equal(1, result.RemovedVersionCount);
        Assert.Equal(2, result.DeletedAssetCount);
        Assert.Null(await unitOfWork.ContentBlocks.GetByIdAsync(target.Id));
        Assert.NotNull(await unitOfWork.ContentBlocks.GetByIdAsync(child.Id));
        Assert.NotNull(await unitOfWork.ContentBlocks.GetByIdAsync(parent.Id));
        Assert.Null(await unitOfWork.ContentBlockVersions.GetByIdAsync(version.Id));
        Assert.Null(await unitOfWork.SectionItems.GetByIdAsync(sectionItem.Id));
        Assert.Empty(await unitOfWork.SectionVariantItems.ListBySectionVariantAsync(variant.Id));
        Assert.Empty(await unitOfWork.AtomicSectionItems.ListByContentBlockAsync(target.Id));
        Assert.Empty(await unitOfWork.ContentBlockRelations.ListChildrenAsync(target.Id));
        Assert.Empty(await unitOfWork.ContentBlockRelations.ListParentsAsync(target.Id));
        Assert.Empty(await unitOfWork.HandoutVersionItems.ListByTargetAsync(HandoutVersionItemTargetType.ContentBlock, target.Id));
        Assert.False(File.Exists(docxPath));
        Assert.False(File.Exists(htmlPath));
    }

    [Fact]
    public async Task DeleteContentBlockCascadeAsync_rejects_active_edit_session()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var target = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "正在编辑", ContentBlockType.KnowledgePoint, Difficulty.Basic));
        var sessionStore = new FakeContentBlockEditSessionStore(
        [
            new ContentBlockEditSession(
                "session-1",
                target.Id,
                SourceContentBlockVersionId: 1,
                EditableDocxPath: "edit.docx",
                OriginalDocxHash: "hash",
                ContentBlockEditSessionStatus.Editing,
                ContentBlockEditLaunchMode.LocalShell,
                OpenedByServer: true,
                Message: null,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow)
        ]);
        var deletion = new ContentBlockDeletionUseCases(
            unitOfWork,
            new LocalContentBlockFileStore(),
            sessionStore);

        var exception = await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => deletion.DeleteContentBlockCascadeAsync(
                new DeleteContentBlockCascadeCommand(Path.GetTempPath(), target.Id)));

        Assert.Equal("ContentBlock has an active Word edit session. Sync or cancel it before deleting.", exception.Message);
        Assert.NotNull(await unitOfWork.ContentBlocks.GetByIdAsync(target.Id));
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
    public async Task CreateAtomicSection_initializes_default_child_blocks_without_docx_versions()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var atomicSections = new AtomicSectionUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var atomicSection = await atomicSections.CreateAtomicSectionAsync(
            new CreateAtomicSectionCommand(
                sectionId,
                "AS Alpha",
                "AS note",
                AtomicSectionType.Custom,
                Difficulty.Advanced,
                AtomicSectionStatus.Draft));

        var items = await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSection.Id);
        var contentBlocks = await unitOfWork.ContentBlocks.ListAsync();
        var versions = await unitOfWork.ContentBlockVersions.ListAsync();

        Assert.Equal(sectionId, atomicSection.SectionId);
        Assert.Equal("AS Alpha", atomicSection.Title);
        Assert.Equal("AS note", atomicSection.Description);
        Assert.Equal(Difficulty.Advanced, atomicSection.Difficulty);
        Assert.Equal([10, 20, 30], items.Select(item => item.SortOrder));
        Assert.Equal(
            [ContentBlockType.KnowledgePoint, ContentBlockType.ExampleGroup, ContentBlockType.ExerciseGroup],
            contentBlocks.OrderBy(block => block.Id).Select(block => block.BlockType));
        Assert.All(contentBlocks, block =>
        {
            Assert.Equal(sectionId, block.SectionId);
            Assert.Equal("AS Alpha", block.Title);
            Assert.Equal(Difficulty.Advanced, block.Difficulty);
            Assert.Null(block.CurrentVersionId);
        });
        Assert.Equal(contentBlocks.OrderBy(block => block.Id).Select(block => block.Id), items.Select(item => item.ContentBlockId));
        Assert.Empty(versions);
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
    public async Task WrapSectionItemsAsAtomicSection_wraps_continuous_content_block_items_transactionally()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sectionUseCases = new SectionUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var firstBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "A", ContentBlockType.KnowledgePoint, "a/v1.docx"));
        var secondBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "B", ContentBlockType.Question, "b/v1.docx"));
        var thirdBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "C", ContentBlockType.ExampleGroup, "c/v1.docx"));
        var fourthBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "D", ContentBlockType.Question, "d/v1.docx"));
        var secondVersion = await unitOfWork.ContentBlockVersions.GetCurrentByContentBlockAsync(secondBlock.Id);

        Assert.NotNull(secondVersion);

        var firstItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, firstBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        var secondItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, secondBlock.Id, ReferenceMode.LockedVersion, secondVersion.Id, SortOrder: 20, TitleOverride: "锁定 B", Note: "保留备注"));
        var thirdItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, thirdBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 30));
        var fourthItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, fourthBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 40));

        var result = await sectionUseCases.WrapSectionItemsAsAtomicSectionAsync(
            new WrapSectionItemsAsAtomicSectionCommand(
                sectionId,
                [secondItem.Id, thirdItem.Id],
                "新 AtomicSection",
                "由连续块升级",
                AtomicSectionType.Custom,
                Difficulty.Medium,
                AtomicSectionStatus.Draft));

        var atomicSection = await unitOfWork.AtomicSections.GetByIdAsync(result.AtomicSectionId);
        var sectionItems = await unitOfWork.SectionItems.ListBySectionAsync(sectionId);
        var atomicItems = await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(result.AtomicSectionId);

        Assert.NotNull(atomicSection);
        Assert.Equal(sectionId, atomicSection.SectionId);
        Assert.Equal("新 AtomicSection", atomicSection.Title);
        Assert.Equal("由连续块升级", atomicSection.Description);
        Assert.Equal(Difficulty.Medium, atomicSection.Difficulty);
        Assert.Equal([secondItem.Id, thirdItem.Id], result.WrappedSectionItemIds);
        Assert.Equal([firstItem.Id, result.SectionItemId, fourthItem.Id], sectionItems.Select(item => item.Id));
        Assert.Equal([10, 20, 30], sectionItems.Select(item => item.SortOrder));
        Assert.Equal(SectionItemTargetType.AtomicSection, sectionItems[1].TargetType);
        Assert.Equal(result.AtomicSectionId, sectionItems[1].TargetId);
        Assert.Equal(SectionStatus.Active, sectionItems[1].Status);
        Assert.Null(await unitOfWork.SectionItems.GetByIdAsync(secondItem.Id));
        Assert.Null(await unitOfWork.SectionItems.GetByIdAsync(thirdItem.Id));
        Assert.Equal([secondBlock.Id, thirdBlock.Id], atomicItems.Select(item => item.ContentBlockId));
        Assert.Equal([ReferenceMode.LockedVersion, ReferenceMode.FollowLatest], atomicItems.Select(item => item.ReferenceMode));
        Assert.Equal(secondVersion.Id, atomicItems[0].LockedContentBlockVersionId);
        Assert.Equal("锁定 B", atomicItems[0].TitleOverride);
        Assert.Equal("保留备注", atomicItems[0].Note);
        Assert.Equal(atomicItems.Select(item => item.Id), result.AtomicSectionItemIds);
        Assert.NotNull(await unitOfWork.ContentBlocks.GetByIdAsync(secondBlock.Id));
        Assert.NotNull(await unitOfWork.ContentBlocks.GetByIdAsync(thirdBlock.Id));
    }

    [Fact]
    public async Task WrapSectionItemsAsAtomicSection_wraps_non_continuous_top_level_content_block_items()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sectionUseCases = new SectionUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var firstBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "A", ContentBlockType.KnowledgePoint, "non-continuous-a/v1.docx"));
        var secondBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "B", ContentBlockType.Question, "non-continuous-b/v1.docx"));
        var thirdBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "C", ContentBlockType.ExampleGroup, "non-continuous-c/v1.docx"));
        var fourthBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "D", ContentBlockType.Question, "non-continuous-d/v1.docx"));

        var firstItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, firstBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        var secondItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, secondBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 20));
        var thirdItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, thirdBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 30));
        var fourthItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, fourthBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 40));

        var result = await sectionUseCases.WrapSectionItemsAsAtomicSectionAsync(
            new WrapSectionItemsAsAtomicSectionCommand(
                sectionId,
                [firstItem.Id, thirdItem.Id],
                "Non-continuous AtomicSection",
                null,
                AtomicSectionType.Custom,
                Difficulty.Medium,
                AtomicSectionStatus.Draft));

        var sectionItems = await unitOfWork.SectionItems.ListBySectionAsync(sectionId);
        var atomicItems = await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(result.AtomicSectionId);

        Assert.Equal([result.SectionItemId, secondItem.Id, fourthItem.Id], sectionItems.Select(item => item.Id));
        Assert.Equal([10, 20, 30], sectionItems.Select(item => item.SortOrder));
        Assert.Equal([firstItem.Id, thirdItem.Id], result.WrappedSectionItemIds);
        Assert.Equal([firstBlock.Id, thirdBlock.Id], atomicItems.Select(item => item.ContentBlockId));
        Assert.Null(await unitOfWork.SectionItems.GetByIdAsync(firstItem.Id));
        Assert.Null(await unitOfWork.SectionItems.GetByIdAsync(thirdItem.Id));
        Assert.NotNull(await unitOfWork.SectionItems.GetByIdAsync(secondItem.Id));
        Assert.NotNull(await unitOfWork.SectionItems.GetByIdAsync(fourthItem.Id));
    }

    [Fact]
    public async Task WrapSectionItemsAsAtomicSection_replaces_section_variant_references_to_wrapped_items()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sectionUseCases = new SectionUseCases(unitOfWork);
        var variantUseCases = new SectionVariantUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var firstBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "A", ContentBlockType.KnowledgePoint, "variant-wrap-a/v1.docx"));
        var secondBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "B", ContentBlockType.Question, "variant-wrap-b/v1.docx"));
        var thirdBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "C", ContentBlockType.Question, "variant-wrap-c/v1.docx"));

        var firstItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, firstBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        var secondItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, secondBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 20));
        var thirdItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, thirdBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 30));

        var variant = await variantUseCases.CreateSectionVariantAsync(
            new CreateSectionVariantCommand(sectionId, "基础讲解版", Difficulty: Difficulty.Basic));
        await variantUseCases.AddSectionVariantItemAsync(
            new AddSectionVariantItemCommand(variant.Id, firstItem.Id, SortOrder: 10, Note: "before"));
        await variantUseCases.AddSectionVariantItemAsync(
            new AddSectionVariantItemCommand(variant.Id, secondItem.Id, SortOrder: 20, Note: "selected"));
        await variantUseCases.AddSectionVariantItemAsync(
            new AddSectionVariantItemCommand(variant.Id, thirdItem.Id, SortOrder: 30, Note: "selected"));

        var result = await sectionUseCases.WrapSectionItemsAsAtomicSectionAsync(
            new WrapSectionItemsAsAtomicSectionCommand(
                sectionId,
                [secondItem.Id, thirdItem.Id],
                "升级片段",
                null,
                AtomicSectionType.Custom,
                Difficulty.Basic,
                AtomicSectionStatus.Draft));

        var variantItems = await unitOfWork.SectionVariantItems.ListBySectionVariantAsync(variant.Id);

        Assert.Equal([firstItem.Id, result.SectionItemId], variantItems.Select(item => item.SectionItemId));
        Assert.Equal([10, 20], variantItems.Select(item => item.SortOrder));
        Assert.Null(await unitOfWork.SectionItems.GetByIdAsync(secondItem.Id));
        Assert.Null(await unitOfWork.SectionItems.GetByIdAsync(thirdItem.Id));
    }

    [Fact]
    public async Task RemoveSectionItem_rejects_item_referenced_by_section_variant_without_changes()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sectionUseCases = new SectionUseCases(unitOfWork);
        var variantUseCases = new SectionVariantUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var block = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "A", ContentBlockType.KnowledgePoint, "variant-delete-a/v1.docx"));
        var sectionItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, block.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        var variant = await variantUseCases.CreateSectionVariantAsync(
            new CreateSectionVariantCommand(
                sectionId,
                "基础讲解版",
                Difficulty: Difficulty.Basic,
                SelectedSectionItemIds: [sectionItem.Id]));

        var error = await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => sectionUseCases.RemoveSectionItemAsync(
                new RemoveSectionItemCommand(sectionId, sectionItem.Id)));

        Assert.Equal(
            "SectionItem is referenced by SectionVariant and cannot be removed.",
            error.Message);
        Assert.NotNull(await unitOfWork.SectionItems.GetByIdAsync(sectionItem.Id));
        Assert.NotNull(await unitOfWork.ContentBlocks.GetByIdAsync(block.Id));
        Assert.Equal(
            [sectionItem.Id],
            (await unitOfWork.SectionVariantItems.ListBySectionVariantAsync(variant.Id)).Select(item => item.SectionItemId));
    }

    [Fact]
    public async Task WrapSectionItemsAsAtomicSection_rejects_atomic_section_selection_without_partial_changes()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sectionUseCases = new SectionUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var firstBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "A", ContentBlockType.KnowledgePoint, "rollback-a/v1.docx"));
        var secondBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "B", ContentBlockType.Question, "rollback-b/v1.docx"));
        var thirdBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(sectionId, "C", ContentBlockType.Question, "rollback-c/v1.docx"));
        var atomicSection = new AtomicSection(sectionId, "Existing AtomicSection");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();

        var firstItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, firstBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        var secondItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.AtomicSection, atomicSection.Id, ReferenceMode.FollowLatest, null, SortOrder: 20));
        var thirdItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, thirdBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 30));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => sectionUseCases.WrapSectionItemsAsAtomicSectionAsync(
                new WrapSectionItemsAsAtomicSectionCommand(
                    sectionId,
                    [firstItem.Id, secondItem.Id],
                    "不应创建",
                    null,
                    AtomicSectionType.Custom,
                    Difficulty.Basic,
                    AtomicSectionStatus.Draft)));

        Assert.Equal([atomicSection.Id], (await unitOfWork.AtomicSections.ListAsync()).Select(item => item.Id));
        Assert.Empty(await unitOfWork.AtomicSectionItems.ListAsync());
        Assert.Equal(
            [firstItem.Id, secondItem.Id, thirdItem.Id],
            (await unitOfWork.SectionItems.ListBySectionAsync(sectionId)).Select(item => item.Id));
    }

    [Fact]
    public async Task SectionVariant_use_cases_require_items_from_the_same_section()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sectionUseCases = new SectionUseCases(unitOfWork);
        var variantUseCases = new SectionVariantUseCases(unitOfWork);

        var topicA = new TeachingTopic("圆周运动");
        var topicB = new TeachingTopic("水平圆周运动");
        await unitOfWork.TeachingTopics.AddAsync(topicA);
        await unitOfWork.TeachingTopics.AddAsync(topicB);
        await unitOfWork.SaveChangesAsync();

        var sectionA = new Section(topicA.Id, "竖直圆轨道");
        var sectionB = new Section(topicB.Id, "水平圆周");
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
            new CreateSectionVariantCommand(sectionA.Id, "课堂版", Difficulty: Difficulty.Basic));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => variantUseCases.AddSectionVariantItemAsync(
                new AddSectionVariantItemCommand(variant.Id, itemB.Id, SortOrder: 1)));

        var variantItem = await variantUseCases.AddSectionVariantItemAsync(
            new AddSectionVariantItemCommand(variant.Id, itemA.Id, SortOrder: 1));

        Assert.True(variantItem.Id > 0);
    }

    [Fact]
    public async Task CreateSectionVariantAsync_creates_variant_items_from_selected_top_level_section_items()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sectionUseCases = new SectionUseCases(unitOfWork);
        var variantUseCases = new SectionVariantUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        await unitOfWork.SectionVariants.AddAsync(new SectionVariant(sectionId, "Existing", sortOrder: 3));
        await unitOfWork.SaveChangesAsync();

        var firstBlock = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "First", ContentBlockType.KnowledgePoint, Difficulty.Basic));
        var secondBlock = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "Second", ContentBlockType.Question, Difficulty.Medium));
        var childBlock = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "Child", ContentBlockType.Explanation, Difficulty.Basic));

        var firstItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, firstBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 20));
        var secondItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, secondBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, childBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 30, ParentItemId: firstItem.Id));

        var created = await variantUseCases.CreateSectionVariantAsync(
            new CreateSectionVariantCommand(
                sectionId,
                "Lecture Medium",
                Type: SectionVariantType.Lecture,
                Difficulty: Difficulty.Medium,
                SelectedSectionItemIds: [firstItem.Id, secondItem.Id]));

        var variant = await unitOfWork.SectionVariants.GetByIdAsync(created.Id);
        var variantItems = await unitOfWork.SectionVariantItems.ListBySectionVariantAsync(created.Id);

        Assert.NotNull(variant);
        Assert.Equal(SectionVariantStatus.Draft, variant.Status);
        Assert.Equal(4, variant.SortOrder);
        Assert.Equal([secondItem.Id, firstItem.Id], variantItems.Select(item => item.SectionItemId));
        Assert.Equal([1, 2], variantItems.Select(item => item.SortOrder));
    }

    [Fact]
    public async Task CreateSectionVariantAsync_allows_empty_selection_and_rejects_unset_or_duplicate_title()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var variantUseCases = new SectionVariantUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var created = await variantUseCases.CreateSectionVariantAsync(
            new CreateSectionVariantCommand(
                sectionId,
                " Empty Variant ",
                Type: SectionVariantType.Review,
                Difficulty: Difficulty.Basic,
                SelectedSectionItemIds: []));

        Assert.Empty(await unitOfWork.SectionVariantItems.ListBySectionVariantAsync(created.Id));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => variantUseCases.CreateSectionVariantAsync(
                new CreateSectionVariantCommand(
                    sectionId,
                    "Unset Variant",
                    Difficulty: Difficulty.Unset,
                    SelectedSectionItemIds: [])));
        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => variantUseCases.CreateSectionVariantAsync(
                new CreateSectionVariantCommand(
                    sectionId,
                    "empty variant",
                    Difficulty: Difficulty.Basic,
                    SelectedSectionItemIds: [])));
    }

    [Fact]
    public async Task DeleteSectionVariantAsync_removes_variant_and_its_items()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sectionUseCases = new SectionUseCases(unitOfWork);
        var variantUseCases = new SectionVariantUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var block = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "Variant Item", ContentBlockType.KnowledgePoint, Difficulty.Basic));
        var sectionItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, block.Id, ReferenceMode.FollowLatest, null, SortOrder: 1));
        var created = await variantUseCases.CreateSectionVariantAsync(
            new CreateSectionVariantCommand(
                sectionId,
                "Variant To Delete",
                Difficulty: Difficulty.Basic,
                SelectedSectionItemIds: [sectionItem.Id]));

        await variantUseCases.DeleteSectionVariantAsync(new DeleteSectionVariantCommand(created.Id));

        Assert.Null(await unitOfWork.SectionVariants.GetByIdAsync(created.Id));
        Assert.Empty(await unitOfWork.SectionVariantItems.ListBySectionVariantAsync(created.Id));
        Assert.NotNull(await unitOfWork.SectionItems.GetByIdAsync(sectionItem.Id));
    }

    [Fact]
    public async Task DeleteSectionVariantAsync_rejects_handout_referenced_variant()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var variantUseCases = new SectionVariantUseCases(unitOfWork);
        var handoutUseCases = new HandoutUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var created = await variantUseCases.CreateSectionVariantAsync(
            new CreateSectionVariantCommand(
                sectionId,
                "Referenced Variant",
                Difficulty: Difficulty.Basic,
                SelectedSectionItemIds: []));
        var handout = new Handout("Variant Handout");
        await unitOfWork.Handouts.AddAsync(handout);
        await unitOfWork.SaveChangesAsync();
        var handoutVersion = await handoutUseCases.CreateHandoutVersionAsync(
            new CreateHandoutVersionCommand(handout.Id, "Variant Handout Version"));
        await handoutUseCases.AddHandoutVersionItemAsync(
            new AddHandoutVersionItemCommand(
                handoutVersion.Id,
                HandoutVersionItemTargetType.SectionVariant,
                created.Id,
                SortOrder: 1));

        var exception = await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => variantUseCases.DeleteSectionVariantAsync(new DeleteSectionVariantCommand(created.Id)));

        Assert.Equal("SectionVariant is referenced by HandoutVersion and cannot be deleted.", exception.Message);
        Assert.NotNull(await unitOfWork.SectionVariants.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task CreateSectionVariantAsync_rejects_invalid_selected_items_without_partial_variant()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sectionUseCases = new SectionUseCases(unitOfWork);
        var variantUseCases = new SectionVariantUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var otherSectionId = await CreateSectionAsync(unitOfWork);

        var activeBlock = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "Active", ContentBlockType.KnowledgePoint, Difficulty.Basic));
        var childBlock = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "Child", ContentBlockType.Explanation, Difficulty.Basic));
        var archivedBlock = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "Archived", ContentBlockType.Question, Difficulty.Basic, Status: ContentBlockStatus.Archived));
        var otherBlock = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(otherSectionId, "Other", ContentBlockType.Question, Difficulty.Basic));
        var archivedAtomicSection = new AtomicSection(sectionId, "Archived Atomic", difficulty: Difficulty.Basic, status: AtomicSectionStatus.Archived);
        await unitOfWork.AtomicSections.AddAsync(archivedAtomicSection);
        await unitOfWork.SaveChangesAsync();

        var topLevelItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, activeBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        var childItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, childBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 20, ParentItemId: topLevelItem.Id));
        var archivedBlockItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, archivedBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 30));
        var archivedSectionItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, activeBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 40, Status: SectionStatus.Archived));
        var archivedAtomicItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.AtomicSection, archivedAtomicSection.Id, ReferenceMode.FollowLatest, null, SortOrder: 50));
        var otherSectionItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(otherSectionId, SectionItemTargetType.ContentBlock, otherBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));

        var invalidSelections = new[]
        {
            new[] { topLevelItem.Id, topLevelItem.Id },
            [999_999],
            [otherSectionItem.Id],
            [childItem.Id],
            [archivedSectionItem.Id],
            [archivedBlockItem.Id],
            [archivedAtomicItem.Id],
        };

        foreach (var selection in invalidSelections)
        {
            await Assert.ThrowsAsync<CmsV2ApplicationException>(
                () => variantUseCases.CreateSectionVariantAsync(
                    new CreateSectionVariantCommand(
                        sectionId,
                        $"Invalid {selection[0]}",
                        Difficulty: Difficulty.Basic,
                        SelectedSectionItemIds: selection)));
        }

        Assert.Empty(await unitOfWork.SectionVariants.ListBySectionAsync(sectionId));
        Assert.Empty(await unitOfWork.SectionVariantItems.ListAsync());
    }

    [Fact]
    public async Task SectionVariant_preview_returns_top_level_candidates_and_default_selection()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sectionUseCases = new SectionUseCases(unitOfWork);
        var variantUseCases = new SectionVariantUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var basicBlock = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "基础知识点", ContentBlockType.KnowledgePoint, Difficulty.Basic));
        var mediumGroup = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "中档例题组", ContentBlockType.ExampleGroup, Difficulty.Medium));
        var advancedBlock = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "提高题", ContentBlockType.Question, Difficulty.Advanced));
        var unsetBlock = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "未设难度", ContentBlockType.GeneralText, Difficulty.Unset));
        var childBlock = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "子级内容", ContentBlockType.Explanation, Difficulty.Basic));
        var atomicSection = new AtomicSection(sectionId, "原子小节", difficulty: Difficulty.Medium);
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();

        var basicItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, basicBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 30));
        var mediumGroupItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, mediumGroup.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        var atomicItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.AtomicSection, atomicSection.Id, ReferenceMode.FollowLatest, null, SortOrder: 20));
        var advancedItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, advancedBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 40));
        var unsetItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, unsetBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 50));
        await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, childBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 60, ParentItemId: basicItem.Id));

        var preview = await variantUseCases.PreviewSectionVariantSelectionAsync(
            new PreviewSectionVariantSelectionCommand(sectionId, Difficulty.Medium));

        Assert.Equal(
            [mediumGroupItem.Id, atomicItem.Id, basicItem.Id, advancedItem.Id, unsetItem.Id],
            preview.Select(candidate => candidate.SectionItemId));
        Assert.All(preview, candidate => Assert.Null(candidate.ParentItemId));
        Assert.Equal(
            [Difficulty.Medium, Difficulty.Medium, Difficulty.Basic, Difficulty.Advanced, Difficulty.Unset],
            preview.Select(candidate => candidate.ResolvedDifficulty));
        Assert.Equal(
            [true, true, true, false, false],
            preview.Select(candidate => candidate.DefaultSelected));
        Assert.Equal(
            [SectionItemTargetType.ContentBlock, SectionItemTargetType.AtomicSection, SectionItemTargetType.ContentBlock, SectionItemTargetType.ContentBlock, SectionItemTargetType.ContentBlock],
            preview.Select(candidate => candidate.TargetType));
        Assert.All(preview, candidate => Assert.True(candidate.Selectable));
    }

    [Fact]
    public async Task SectionVariant_preview_marks_archived_items_and_targets_unselectable()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sectionUseCases = new SectionUseCases(unitOfWork);
        var variantUseCases = new SectionVariantUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        var activeBlock = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "可用知识点", ContentBlockType.KnowledgePoint, Difficulty.Basic));
        var archivedBlock = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "归档知识点", ContentBlockType.KnowledgePoint, Difficulty.Basic, Status: ContentBlockStatus.Archived));
        var archivedAtomicSection = new AtomicSection(sectionId, "归档原子小节", difficulty: Difficulty.Basic, status: AtomicSectionStatus.Archived);
        await unitOfWork.AtomicSections.AddAsync(archivedAtomicSection);
        await unitOfWork.SaveChangesAsync();

        var archivedItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, activeBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 10, Status: SectionStatus.Archived));
        var archivedBlockItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, archivedBlock.Id, ReferenceMode.FollowLatest, null, SortOrder: 20));
        var archivedAtomicItem = await sectionUseCases.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.AtomicSection, archivedAtomicSection.Id, ReferenceMode.FollowLatest, null, SortOrder: 30));

        var preview = await variantUseCases.PreviewSectionVariantSelectionAsync(
            new PreviewSectionVariantSelectionCommand(sectionId, Difficulty.Basic));

        Assert.Equal([archivedItem.Id, archivedBlockItem.Id, archivedAtomicItem.Id], preview.Select(candidate => candidate.SectionItemId));
        Assert.Equal([false, false, false], preview.Select(candidate => candidate.Selectable));
        Assert.Equal([false, false, false], preview.Select(candidate => candidate.DefaultSelected));
        Assert.Equal(
            ["SectionItem is archived.", "ContentBlock is archived.", "AtomicSection is archived."],
            preview.Select(candidate => candidate.UnavailableReason));
    }

    [Fact]
    public async Task SectionVariant_preview_rejects_missing_section_and_unset_difficulty()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var variantUseCases = new SectionVariantUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => variantUseCases.PreviewSectionVariantSelectionAsync(
                new PreviewSectionVariantSelectionCommand(sectionId, Difficulty.Unset)));
        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => variantUseCases.PreviewSectionVariantSelectionAsync(
                new PreviewSectionVariantSelectionCommand(999_999, Difficulty.Basic)));
    }

    [Fact]
    public async Task Handout_use_cases_create_versions_and_reject_section_targets()
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

    private sealed class FakeContentBlockEditSessionStore(
        IReadOnlyList<ContentBlockEditSession>? activeSessions = null)
        : IContentBlockEditSessionStore
    {
        public Task SaveAsync(
            string bankRootDirectory,
            ContentBlockEditSession session,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<ContentBlockEditSession?> GetAsync(
            string bankRootDirectory,
            string sessionId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ContentBlockEditSession?>(null);
        }

        public Task<IReadOnlyList<ContentBlockEditSession>> ListActiveAsync(
            string bankRootDirectory,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(activeSessions ?? []);
        }
    }
}
