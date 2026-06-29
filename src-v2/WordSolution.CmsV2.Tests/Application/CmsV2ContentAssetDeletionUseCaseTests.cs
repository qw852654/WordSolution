using Microsoft.EntityFrameworkCore;
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

public sealed class CmsV2ContentAssetDeletionUseCaseTests
{
    [Fact]
    public async Task DeleteSectionItemContentAssetAsync_removes_section_reference_variant_reference_block_version_and_files()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sections = new SectionUseCases(unitOfWork);
        var variants = new SectionVariantUseCases(unitOfWork);
        var deletion = CreateDeletionUseCases(unitOfWork);
        var bankRootDirectory = CreateTempBankRoot();
        var sectionId = await CreateSectionAsync(unitOfWork);
        var block = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "待删除题目", ContentBlockType.Question, Difficulty.Basic));
        var paths = CreateVersionFiles(bankRootDirectory, block.Id);
        var version = await contentBlocks.CreateContentBlockVersionAsync(
            new CreateContentBlockVersionCommand(
                block.Id,
                paths.DocxPath,
                paths.HtmlPath,
                PlainText: "题干",
                SetAsCurrent: true));
        var sectionItem = await sections.AddSectionItemAsync(
            new AddSectionItemCommand(
                sectionId,
                SectionItemTargetType.ContentBlock,
                block.Id,
                ReferenceMode.FollowLatest,
                null,
                SortOrder: 10));
        var variant = await variants.CreateSectionVariantAsync(
            new CreateSectionVariantCommand(
                sectionId,
                "基础版",
                Difficulty: Difficulty.Basic,
                SelectedSectionItemIds: [sectionItem.Id]));

        var result = await deletion.DeleteSectionItemContentAssetAsync(
            new DeleteSectionItemContentAssetCommand(
                bankRootDirectory,
                sectionId,
                sectionItem.Id));

        Assert.Equal(block.Id, result.RootContentBlockId);
        Assert.True(result.RemovedCurrentReference);
        Assert.True(result.DeletedRootAsset);
        Assert.Equal(1, result.RemovedSectionItemCount);
        Assert.Equal(1, result.RemovedSectionVariantItemCount);
        Assert.Equal(0, result.RemovedAtomicSectionItemCount);
        Assert.Equal(1, result.DeletedContentBlockCount);
        Assert.Equal(1, result.DeletedContentBlockVersionCount);
        Assert.Equal(2, result.DeletedFileCount);
        Assert.Empty(result.RetainReasons);
        Assert.Null(await unitOfWork.SectionItems.GetByIdAsync(sectionItem.Id));
        Assert.Empty(await unitOfWork.SectionVariantItems.ListBySectionVariantAsync(variant.Id));
        Assert.Null(await unitOfWork.ContentBlocks.GetByIdAsync(block.Id));
        Assert.Null(await unitOfWork.ContentBlockVersions.GetByIdAsync(version.Id));
        Assert.False(File.Exists(paths.DocxPath));
        Assert.False(File.Exists(paths.HtmlPath));
    }

    [Fact]
    public async Task DeleteAtomicSectionItemContentAssetAsync_removes_atomic_reference_and_unprotected_content_block()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var deletion = CreateDeletionUseCases(unitOfWork);
        var bankRootDirectory = CreateTempBankRoot();
        var sectionId = await CreateSectionAsync(unitOfWork);
        var block = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "原子小节题目", ContentBlockType.Question, Difficulty.Basic));
        var atomicSection = new AtomicSection(sectionId, "原子小节", difficulty: Difficulty.Basic);
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();
        var atomicItem = new AtomicSectionItem(
            atomicSection.Id,
            block.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 10);
        await unitOfWork.AtomicSectionItems.AddAsync(atomicItem);
        await unitOfWork.SaveChangesAsync();

        var result = await deletion.DeleteAtomicSectionItemContentAssetAsync(
            new DeleteAtomicSectionItemContentAssetCommand(
                bankRootDirectory,
                atomicSection.Id,
                atomicItem.Id));

        Assert.Equal(block.Id, result.RootContentBlockId);
        Assert.True(result.RemovedCurrentReference);
        Assert.True(result.DeletedRootAsset);
        Assert.Equal(0, result.RemovedSectionItemCount);
        Assert.Equal(0, result.RemovedSectionVariantItemCount);
        Assert.Equal(1, result.RemovedAtomicSectionItemCount);
        Assert.Equal(1, result.DeletedContentBlockCount);
        Assert.Null(await unitOfWork.AtomicSectionItems.GetByIdAsync(atomicItem.Id));
        Assert.Null(await unitOfWork.ContentBlocks.GetByIdAsync(block.Id));
    }

    [Fact]
    public async Task DeleteSectionItemContentAssetAsync_rejects_atomic_section_targets()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var sections = new SectionUseCases(unitOfWork);
        var deletion = CreateDeletionUseCases(unitOfWork);
        var bankRootDirectory = CreateTempBankRoot();
        var sectionId = await CreateSectionAsync(unitOfWork);
        var atomicSection = new AtomicSection(sectionId, "不支持删除的原子小节", difficulty: Difficulty.Basic);
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();
        var sectionItem = await sections.AddSectionItemAsync(
            new AddSectionItemCommand(
                sectionId,
                SectionItemTargetType.AtomicSection,
                atomicSection.Id,
                ReferenceMode.FollowLatest,
                null,
                SortOrder: 10));

        var exception = await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => deletion.DeleteSectionItemContentAssetAsync(
                new DeleteSectionItemContentAssetCommand(
                    bankRootDirectory,
                    sectionId,
                    sectionItem.Id)));

        Assert.Equal("Deleting AtomicSection assets from SectionItem is not supported in this phase.", exception.Message);
        Assert.NotNull(await unitOfWork.SectionItems.GetByIdAsync(sectionItem.Id));
        Assert.NotNull(await unitOfWork.AtomicSections.GetByIdAsync(atomicSection.Id));
    }

    [Fact]
    public async Task DeleteSectionItemContentAssetAsync_recursively_deletes_unprotected_children()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var relations = new ContentBlockRelationUseCases(unitOfWork);
        var sections = new SectionUseCases(unitOfWork);
        var deletion = CreateDeletionUseCases(unitOfWork);
        var bankRootDirectory = CreateTempBankRoot();
        var sectionId = await CreateSectionAsync(unitOfWork);
        var root = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "父块", ContentBlockType.ExampleGroup, Difficulty.Basic));
        var child = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "子块", ContentBlockType.Question, Difficulty.Basic));
        var grandChild = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "孙块", ContentBlockType.Question, Difficulty.Basic));
        var rootVersion = await CreateVersionAsync(contentBlocks, bankRootDirectory, root.Id);
        var childVersion = await CreateVersionAsync(contentBlocks, bankRootDirectory, child.Id);
        var grandChildVersion = await CreateVersionAsync(contentBlocks, bankRootDirectory, grandChild.Id);
        var rootChildRelation = await relations.AddContentBlockRelationAsync(
            new AddContentBlockRelationCommand(root.Id, child.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        var childGrandChildRelation = await relations.AddContentBlockRelationAsync(
            new AddContentBlockRelationCommand(child.Id, grandChild.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        var sectionItem = await sections.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, root.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));

        var result = await deletion.DeleteSectionItemContentAssetAsync(
            new DeleteSectionItemContentAssetCommand(bankRootDirectory, sectionId, sectionItem.Id));

        Assert.True(result.DeletedRootAsset);
        Assert.Equal(3, result.DeletedContentBlockCount);
        Assert.Equal(3, result.DeletedContentBlockVersionCount);
        Assert.Equal(2, result.RemovedContentBlockRelationCount);
        Assert.Equal(6, result.DeletedFileCount);
        Assert.Null(await unitOfWork.ContentBlocks.GetByIdAsync(root.Id));
        Assert.Null(await unitOfWork.ContentBlocks.GetByIdAsync(child.Id));
        Assert.Null(await unitOfWork.ContentBlocks.GetByIdAsync(grandChild.Id));
        Assert.Null(await unitOfWork.ContentBlockVersions.GetByIdAsync(rootVersion.Id));
        Assert.Null(await unitOfWork.ContentBlockVersions.GetByIdAsync(childVersion.Id));
        Assert.Null(await unitOfWork.ContentBlockVersions.GetByIdAsync(grandChildVersion.Id));
        Assert.Null(await unitOfWork.ContentBlockRelations.GetByIdAsync(rootChildRelation.Id));
        Assert.Null(await unitOfWork.ContentBlockRelations.GetByIdAsync(childGrandChildRelation.Id));
    }

    [Theory]
    [InlineData("section")]
    [InlineData("atomic-section")]
    [InlineData("handout")]
    public async Task DeleteSectionItemContentAssetAsync_removes_parent_relation_and_retains_child_referenced_elsewhere(
        string protectingReference)
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var relations = new ContentBlockRelationUseCases(unitOfWork);
        var sections = new SectionUseCases(unitOfWork);
        var deletion = CreateDeletionUseCases(unitOfWork);
        var bankRootDirectory = CreateTempBankRoot();
        var sectionId = await CreateSectionAsync(unitOfWork);
        var root = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "父块", ContentBlockType.ExampleGroup, Difficulty.Basic));
        var child = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "受保护子块", ContentBlockType.Question, Difficulty.Basic));
        var relation = await relations.AddContentBlockRelationAsync(
            new AddContentBlockRelationCommand(root.Id, child.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        var sectionItem = await sections.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, root.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        await AddProtectingReferenceAsync(unitOfWork, sections, sectionId, child.Id, protectingReference);

        var result = await deletion.DeleteSectionItemContentAssetAsync(
            new DeleteSectionItemContentAssetCommand(bankRootDirectory, sectionId, sectionItem.Id));

        Assert.True(result.DeletedRootAsset);
        Assert.Equal(1, result.DeletedContentBlockCount);
        Assert.Equal(1, result.RemovedContentBlockRelationCount);
        Assert.Contains(result.RetainReasons, reason => reason.ContentBlockId == child.Id);
        Assert.Null(await unitOfWork.ContentBlocks.GetByIdAsync(root.Id));
        Assert.NotNull(await unitOfWork.ContentBlocks.GetByIdAsync(child.Id));
        Assert.Null(await unitOfWork.ContentBlockRelations.GetByIdAsync(relation.Id));
    }

    [Fact]
    public async Task DeleteSectionItemContentAssetAsync_retains_root_referenced_by_external_relation()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var relations = new ContentBlockRelationUseCases(unitOfWork);
        var sections = new SectionUseCases(unitOfWork);
        var deletion = CreateDeletionUseCases(unitOfWork);
        var bankRootDirectory = CreateTempBankRoot();
        var sectionId = await CreateSectionAsync(unitOfWork);
        var externalParent = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "外部父块", ContentBlockType.ExerciseGroup, Difficulty.Basic));
        var root = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "被外部 relation 引用", ContentBlockType.Question, Difficulty.Basic));
        var relation = await relations.AddContentBlockRelationAsync(
            new AddContentBlockRelationCommand(externalParent.Id, root.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        var sectionItem = await sections.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, root.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));

        var result = await deletion.DeleteSectionItemContentAssetAsync(
            new DeleteSectionItemContentAssetCommand(bankRootDirectory, sectionId, sectionItem.Id));

        Assert.False(result.DeletedRootAsset);
        Assert.Equal(0, result.DeletedContentBlockCount);
        Assert.Equal(0, result.RemovedContentBlockRelationCount);
        Assert.Contains(result.RetainReasons, reason =>
            reason.ContentBlockId == root.Id && reason.ReasonCode == "ReferencedByRelation");
        Assert.Null(await unitOfWork.SectionItems.GetByIdAsync(sectionItem.Id));
        Assert.NotNull(await unitOfWork.ContentBlocks.GetByIdAsync(root.Id));
        Assert.NotNull(await unitOfWork.ContentBlockRelations.GetByIdAsync(relation.Id));
    }

    [Fact]
    public async Task DeleteSectionItemContentAssetAsync_handles_relation_cycles_without_infinite_recursion()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sections = new SectionUseCases(unitOfWork);
        var deletion = CreateDeletionUseCases(unitOfWork);
        var bankRootDirectory = CreateTempBankRoot();
        var sectionId = await CreateSectionAsync(unitOfWork);
        var root = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "循环父块", ContentBlockType.ExampleGroup, Difficulty.Basic));
        var child = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "循环子块", ContentBlockType.Question, Difficulty.Basic));
        var rootToChild = new ContentBlockRelation(root.Id, child.Id, ReferenceMode.FollowLatest, null, 10);
        var childToRoot = new ContentBlockRelation(child.Id, root.Id, ReferenceMode.FollowLatest, null, 10);
        await unitOfWork.ContentBlockRelations.AddAsync(rootToChild);
        await unitOfWork.ContentBlockRelations.AddAsync(childToRoot);
        await unitOfWork.SaveChangesAsync();
        var sectionItem = await sections.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, root.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));

        var result = await deletion.DeleteSectionItemContentAssetAsync(
            new DeleteSectionItemContentAssetCommand(bankRootDirectory, sectionId, sectionItem.Id));

        Assert.True(result.DeletedRootAsset);
        Assert.Equal(2, result.DeletedContentBlockCount);
        Assert.Equal(2, result.RemovedContentBlockRelationCount);
        Assert.Null(await unitOfWork.ContentBlocks.GetByIdAsync(root.Id));
        Assert.Null(await unitOfWork.ContentBlocks.GetByIdAsync(child.Id));
        Assert.Empty(await unitOfWork.ContentBlockRelations.ListAsync());
    }

    [Fact]
    public async Task DeleteSectionItemContentAssetAsync_rejects_active_edit_session_before_removing_current_reference()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var relations = new ContentBlockRelationUseCases(unitOfWork);
        var sections = new SectionUseCases(unitOfWork);
        var bankRootDirectory = CreateTempBankRoot();
        var sectionId = await CreateSectionAsync(unitOfWork);
        var root = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "父块", ContentBlockType.ExampleGroup, Difficulty.Basic));
        var child = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "正在编辑的子块", ContentBlockType.Question, Difficulty.Basic));
        await relations.AddContentBlockRelationAsync(
            new AddContentBlockRelationCommand(root.Id, child.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        var sectionItem = await sections.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, root.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));
        var deletion = CreateDeletionUseCases(
            unitOfWork,
            new FakeContentBlockEditSessionStore(
            [
                CreateActiveEditSession(child.Id)
            ]));

        var exception = await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => deletion.DeleteSectionItemContentAssetAsync(
                new DeleteSectionItemContentAssetCommand(bankRootDirectory, sectionId, sectionItem.Id)));

        Assert.Equal("ContentBlock has an active Word edit session. Sync or cancel it before deleting.", exception.Message);
        Assert.NotNull(await unitOfWork.SectionItems.GetByIdAsync(sectionItem.Id));
        Assert.NotNull(await unitOfWork.ContentBlocks.GetByIdAsync(root.Id));
        Assert.NotNull(await unitOfWork.ContentBlocks.GetByIdAsync(child.Id));
    }

    [Fact]
    public async Task DeleteSectionItemContentAssetAsync_cleans_tag_and_teaching_note_bindings_for_deleted_blocks()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sections = new SectionUseCases(unitOfWork);
        var deletion = CreateDeletionUseCases(unitOfWork);
        var bankRootDirectory = CreateTempBankRoot();
        var sectionId = await CreateSectionAsync(unitOfWork);
        var otherSectionId = await CreateSectionAsync(unitOfWork);
        var block = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "有标签和评注的块", ContentBlockType.Question, Difficulty.Basic));
        var otherBlock = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(otherSectionId, "保留评注绑定的块", ContentBlockType.Question, Difficulty.Basic));
        var tag = new Tag("待清理", "tag-blue");
        await unitOfWork.Tags.AddAsync(tag);
        await unitOfWork.SaveChangesAsync();
        var tagBinding = new TagBinding(tag.Id, TagBindingTargetType.ContentBlock, block.Id);
        await unitOfWork.TagBindings.AddAsync(tagBinding);
        var orphanedNote = new TeachingNote(TeachingNoteType.General, "删除后无绑定");
        var retainedNote = new TeachingNote(TeachingNoteType.General, "删除后仍有绑定");
        await unitOfWork.TeachingNotes.AddAsync(orphanedNote);
        await unitOfWork.TeachingNotes.AddAsync(retainedNote);
        await unitOfWork.SaveChangesAsync();
        var orphanedNoteBinding = new TeachingNoteBinding(orphanedNote.Id, TeachingNoteBindingTargetType.ContentBlock, block.Id);
        var removedRetainedNoteBinding = new TeachingNoteBinding(retainedNote.Id, TeachingNoteBindingTargetType.ContentBlock, block.Id);
        var preservedRetainedNoteBinding = new TeachingNoteBinding(retainedNote.Id, TeachingNoteBindingTargetType.ContentBlock, otherBlock.Id);
        await unitOfWork.TeachingNoteBindings.AddAsync(orphanedNoteBinding);
        await unitOfWork.TeachingNoteBindings.AddAsync(removedRetainedNoteBinding);
        await unitOfWork.TeachingNoteBindings.AddAsync(preservedRetainedNoteBinding);
        await unitOfWork.SaveChangesAsync();
        var sectionItem = await sections.AddSectionItemAsync(
            new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, block.Id, ReferenceMode.FollowLatest, null, SortOrder: 10));

        var result = await deletion.DeleteSectionItemContentAssetAsync(
            new DeleteSectionItemContentAssetCommand(bankRootDirectory, sectionId, sectionItem.Id));

        Assert.True(result.DeletedRootAsset);
        Assert.Null(await unitOfWork.TagBindings.GetByIdAsync(tagBinding.Id));
        Assert.NotNull(await unitOfWork.Tags.GetByIdAsync(tag.Id));
        Assert.Null(await unitOfWork.TeachingNoteBindings.GetByIdAsync(orphanedNoteBinding.Id));
        Assert.Null(await unitOfWork.TeachingNoteBindings.GetByIdAsync(removedRetainedNoteBinding.Id));
        Assert.NotNull(await unitOfWork.TeachingNoteBindings.GetByIdAsync(preservedRetainedNoteBinding.Id));
        Assert.Null(await unitOfWork.TeachingNotes.GetByIdAsync(orphanedNote.Id));
        Assert.NotNull(await unitOfWork.TeachingNotes.GetByIdAsync(retainedNote.Id));
    }

    private static ContentAssetDeletionUseCases CreateDeletionUseCases(EfCmsV2UnitOfWork unitOfWork)
    {
        return CreateDeletionUseCases(unitOfWork, new FakeContentBlockEditSessionStore());
    }

    private static ContentAssetDeletionUseCases CreateDeletionUseCases(
        EfCmsV2UnitOfWork unitOfWork,
        IContentBlockEditSessionStore editSessionStore)
    {
        return new ContentAssetDeletionUseCases(
            unitOfWork,
            new LocalContentBlockFileStore(),
            editSessionStore);
    }

    private static async Task<CmsV2DbContext> CreateMigratedContextAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-content-asset-deletion-tests",
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

    private static string CreateTempBankRoot()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "cms-v2-content-asset-deletion-assets",
            Guid.NewGuid().ToString("N"));
    }

    private static (string DocxPath, string HtmlPath) CreateVersionFiles(string bankRootDirectory, int contentBlockId)
    {
        var docxPath = Path.Combine(bankRootDirectory, "content-blocks", "source", contentBlockId.ToString(), "v1.docx");
        var htmlPath = Path.Combine(bankRootDirectory, "content-blocks", "html", contentBlockId.ToString(), "v1.html");
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(htmlPath)!);
        File.WriteAllText(docxPath, "docx placeholder");
        File.WriteAllText(htmlPath, "<html></html>");

        return (docxPath, htmlPath);
    }

    private static async Task<CreatedEntityResult> CreateVersionAsync(
        ContentBlockUseCases contentBlocks,
        string bankRootDirectory,
        int contentBlockId)
    {
        var paths = CreateVersionFiles(bankRootDirectory, contentBlockId);
        return await contentBlocks.CreateContentBlockVersionAsync(
            new CreateContentBlockVersionCommand(
                contentBlockId,
                paths.DocxPath,
                paths.HtmlPath,
                SetAsCurrent: true));
    }

    private static async Task AddProtectingReferenceAsync(
        EfCmsV2UnitOfWork unitOfWork,
        SectionUseCases sections,
        int sectionId,
        int contentBlockId,
        string protectingReference)
    {
        if (protectingReference == "section")
        {
            await sections.AddSectionItemAsync(
                new AddSectionItemCommand(sectionId, SectionItemTargetType.ContentBlock, contentBlockId, ReferenceMode.FollowLatest, null, SortOrder: 20));
            return;
        }

        if (protectingReference == "atomic-section")
        {
            var atomicSection = new AtomicSection(sectionId, "保护子块的原子小节", difficulty: Difficulty.Basic);
            await unitOfWork.AtomicSections.AddAsync(atomicSection);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.AtomicSectionItems.AddAsync(
                new AtomicSectionItem(
                    atomicSection.Id,
                    contentBlockId,
                    ReferenceMode.FollowLatest,
                    lockedContentBlockVersionId: null,
                    sortOrder: 10));
            await unitOfWork.SaveChangesAsync();
            return;
        }

        var handout = new Handout("保护子块的讲义");
        await unitOfWork.Handouts.AddAsync(handout);
        await unitOfWork.SaveChangesAsync();
        var handouts = new HandoutUseCases(unitOfWork);
        var handoutVersion = await handouts.CreateHandoutVersionAsync(
            new CreateHandoutVersionCommand(handout.Id, "讲义版本"));
        await handouts.AddHandoutVersionItemAsync(
            new AddHandoutVersionItemCommand(
                handoutVersion.Id,
                HandoutVersionItemTargetType.ContentBlock,
                contentBlockId,
                SortOrder: 10));
    }

    private static ContentBlockEditSession CreateActiveEditSession(int contentBlockId)
    {
        return new ContentBlockEditSession(
            $"session-{contentBlockId}",
            contentBlockId,
            SourceContentBlockVersionId: 1,
            EditableDocxPath: "edit.docx",
            OriginalDocxHash: "hash",
            ContentBlockEditSessionStatus.Editing,
            ContentBlockEditLaunchMode.LocalShell,
            OpenedByServer: true,
            Message: null,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);
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
