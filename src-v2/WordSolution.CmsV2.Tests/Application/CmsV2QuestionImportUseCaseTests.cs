using Aspose.Words;
using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Application.ContentBlocks;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Infrastructure.Documents;
using WordSolution.CmsV2.Infrastructure.Persistence;
using WordSolution.CmsV2.Infrastructure.Repositories;
using DomainSection = WordSolution.CmsV2.Domain.Entities.Section;

namespace WordSolution.CmsV2.Tests.Application;

public sealed class CmsV2QuestionImportUseCaseTests
{
    [Fact]
    public async Task CreateSession_creates_temporary_word_session_without_formal_content_and_launches_when_requested()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var bankRootDirectory = CreateTempRoot();
        var launcher = new FakeQuestionImportSessionLauncher();
        var closeChecker = new FakeQuestionImportDocumentCloseChecker();
        var useCases = CreateUseCases(unitOfWork, launcher, closeChecker);

        var session = await useCases.CreateSessionAsync(
            new CreateQuestionImportSessionCommand(
                bankRootDirectory,
                new InsertQuestionContext(
                    sectionId,
                    AtomicSectionId: null,
                    AtomicSectionPanelId: null,
                    AfterAtomicSectionItemId: null,
                    AfterSectionItemId: null,
                    DefaultTeachingRole: AtomicSectionTeachingRole.Unclassified,
                    DefaultDifficulty: Difficulty.Basic),
                OpenWord: true));

        var sessionDirectory = Path.Combine(bankRootDirectory, "edit-sessions", "question-imports", session.SessionId);

        Assert.Equal(QuestionImportSessionStatus.Editing, session.Status);
        Assert.Equal(sectionId, session.Context.SectionId);
        Assert.Empty(session.Candidates);
        Assert.True(File.Exists(Path.Combine(sessionDirectory, "source.docx")));
        Assert.Equal(Path.Combine(sessionDirectory, "source.docx"), launcher.OpenedDocxPaths.Single());
        Assert.Empty(await unitOfWork.ContentBlocks.ListAsync());
        Assert.Empty(await unitOfWork.ContentBlockVersions.ListAsync());
        Assert.Empty(await unitOfWork.SectionItems.ListBySectionAsync(sectionId));
    }

    [Fact]
    public async Task GetSession_when_source_word_is_closed_splits_candidates_and_enters_ready_for_review()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var bankRootDirectory = CreateTempRoot();
        var closeChecker = new FakeQuestionImportDocumentCloseChecker { IsClosed = true };
        var useCases = CreateUseCases(unitOfWork, new FakeQuestionImportSessionLauncher(), closeChecker);

        var session = await useCases.CreateSessionAsync(
            new CreateQuestionImportSessionCommand(
                bankRootDirectory,
                new InsertQuestionContext(
                    sectionId,
                    null,
                    null,
                    null,
                    null,
                    AtomicSectionTeachingRole.Unclassified,
                    Difficulty.Medium),
                OpenWord: false));
        CreateMultiQuestionDocx(Path.Combine(bankRootDirectory, "edit-sessions", "question-imports", session.SessionId, "source.docx"));

        var readySession = await useCases.GetSessionAsync(
            new GetQuestionImportSessionCommand(bankRootDirectory, session.SessionId));

        Assert.Equal(QuestionImportSessionStatus.ReadyForReview, readySession.Status);
        Assert.Equal(3, readySession.Candidates.Count);
        Assert.All(readySession.Candidates, candidate =>
        {
            Assert.Equal(ContentBlockPartParseStatus.Parsed, candidate.ParseStatus);
            Assert.NotNull(candidate.HtmlPreview);
            Assert.Contains("data-question-part=\"Stem\"", candidate.HtmlPreview);
        });
        Assert.DoesNotContain("导入前说明", string.Join("\n", readySession.Candidates.Select(candidate => candidate.HtmlPreview)));
    }

    [Fact]
    public async Task Reopen_reopens_source_word_and_returns_to_editing()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var bankRootDirectory = CreateTempRoot();
        var launcher = new FakeQuestionImportSessionLauncher();
        var closeChecker = new FakeQuestionImportDocumentCloseChecker { IsClosed = true };
        var useCases = CreateUseCases(unitOfWork, launcher, closeChecker);

        var session = await useCases.CreateSessionAsync(
            new CreateQuestionImportSessionCommand(
                bankRootDirectory,
                new InsertQuestionContext(sectionId, null, null, null, null, AtomicSectionTeachingRole.Unclassified, Difficulty.Basic),
                OpenWord: false));
        CreateMultiQuestionDocx(Path.Combine(bankRootDirectory, "edit-sessions", "question-imports", session.SessionId, "source.docx"));
        _ = await useCases.GetSessionAsync(new GetQuestionImportSessionCommand(bankRootDirectory, session.SessionId));

        var reopened = await useCases.ReopenSessionAsync(
            new ReopenQuestionImportSessionCommand(bankRootDirectory, session.SessionId));

        Assert.Equal(QuestionImportSessionStatus.Editing, reopened.Status);
        Assert.Single(launcher.OpenedDocxPaths);
    }

    [Fact]
    public async Task ConfirmImport_batch_creates_formal_question_versions_and_section_items()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var bankRootDirectory = CreateTempRoot();
        var useCases = CreateUseCases(
            unitOfWork,
            new FakeQuestionImportSessionLauncher(),
            new FakeQuestionImportDocumentCloseChecker { IsClosed = true });

        var session = await CreateReadySessionAsync(useCases, bankRootDirectory, sectionId);

        var result = await useCases.ConfirmAsync(
            new ConfirmQuestionImportCommand(
                bankRootDirectory,
                session.SessionId,
                [
                    new ConfirmQuestionImportCandidateSelection(session.Candidates[0].CandidateId, Selected: true, Title: "导入题目 1"),
                    new ConfirmQuestionImportCandidateSelection(session.Candidates[1].CandidateId, Selected: false, Title: "跳过"),
                    new ConfirmQuestionImportCandidateSelection(session.Candidates[2].CandidateId, Selected: true, Title: string.Empty)
                ]));

        var blocks = await unitOfWork.ContentBlocks.ListAsync();
        var versions = await unitOfWork.ContentBlockVersions.ListAsync();
        var parts = await unitOfWork.ContentBlockVersionParts.ListAsync();
        var sectionItems = await unitOfWork.SectionItems.ListBySectionAsync(sectionId);

        Assert.Equal(2, result.ContentBlockIds.Count);
        Assert.Equal(2, result.ContentBlockVersionIds.Count);
        Assert.Equal(2, result.SectionItemIds.Count);
        Assert.Empty(result.AtomicSectionItemIds);
        Assert.Equal(result.SectionItemIds[0], result.FirstInsertedNodeId);
        Assert.Equal("SectionItem", result.FirstInsertedNodeType);
        Assert.Equal(2, blocks.Count);
        Assert.All(blocks, block =>
        {
            Assert.Equal(ContentBlockType.Question, block.BlockType);
            Assert.Equal(QuestionType.Unset, block.QuestionType);
            Assert.Equal(Difficulty.Medium, block.Difficulty);
        });
        Assert.Equal(["导入题目 1", string.Empty], blocks.OrderBy(block => block.Id).Select(block => block.Title));
        Assert.Equal(2, versions.Count);
        Assert.Equal(2, sectionItems.Count);
        Assert.All(sectionItems, item => Assert.Equal(SectionItemTargetType.ContentBlock, item.TargetType));
        Assert.Contains(parts, part => part.PartType == ContentBlockPartType.Answer);
        Assert.All(versions, version => Assert.True(File.Exists(version.DocxPath)));
    }

    [Fact]
    public async Task ConfirmImport_with_atomic_section_context_creates_atomic_section_items()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var atomicSection = new AtomicSection(sectionId, "AS");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();
        var bankRootDirectory = CreateTempRoot();
        var useCases = CreateUseCases(
            unitOfWork,
            new FakeQuestionImportSessionLauncher(),
            new FakeQuestionImportDocumentCloseChecker { IsClosed = true });

        var session = await CreateReadySessionAsync(
            useCases,
            bankRootDirectory,
            sectionId,
            atomicSection.Id,
            AtomicSectionTeachingRole.Example,
            Difficulty.Advanced);

        var result = await useCases.ConfirmAsync(
            new ConfirmQuestionImportCommand(
                bankRootDirectory,
                session.SessionId,
                [new ConfirmQuestionImportCandidateSelection(session.Candidates[0].CandidateId, Selected: true, Title: "AS 题目")]));

        var items = await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSection.Id);
        var block = await unitOfWork.ContentBlocks.GetByIdAsync(result.ContentBlockIds.Single());

        Assert.Empty(result.SectionItemIds);
        Assert.Single(result.AtomicSectionItemIds);
        Assert.Equal(result.AtomicSectionItemIds.Single(), result.FirstInsertedNodeId);
        Assert.Equal("AtomicSectionItem", result.FirstInsertedNodeType);
        Assert.Single(items);
        Assert.Equal(AtomicSectionTeachingRole.Example, items.Single().TeachingRole);
        Assert.Null(items.Single().AtomicSectionPanelId);
        Assert.Equal(Difficulty.Advanced, block!.Difficulty);
    }

    [Fact]
    public async Task ConfirmImport_with_atomic_section_panel_context_creates_panel_items_and_orders_within_panel()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var atomicSection = new AtomicSection(sectionId, "AS");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();

        var panel = new AtomicSectionPanel(
            atomicSection.Id,
            "例题 panel",
            AtomicSectionTeachingRole.Example,
            Difficulty.Basic,
            sortOrder: 10);
        await unitOfWork.AtomicSectionPanels.AddAsync(panel);
        await unitOfWork.SaveChangesAsync();

        var existingPanelBlock = await CreateContentBlockAsync(unitOfWork, sectionId, "既有 panel 题", Difficulty.Basic);
        var existingPanelItem = new AtomicSectionItem(
            atomicSection.Id,
            existingPanelBlock.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 10,
            atomicSectionPanelId: panel.Id,
            teachingRole: AtomicSectionTeachingRole.Example);
        await unitOfWork.AtomicSectionItems.AddAsync(existingPanelItem);

        var existingUnassignedBlock = await CreateContentBlockAsync(unitOfWork, sectionId, "未归组题", Difficulty.Unset);
        var existingUnassignedItem = new AtomicSectionItem(
            atomicSection.Id,
            existingUnassignedBlock.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 10,
            teachingRole: AtomicSectionTeachingRole.Unclassified);
        await unitOfWork.AtomicSectionItems.AddAsync(existingUnassignedItem);
        await unitOfWork.SaveChangesAsync();

        var bankRootDirectory = CreateTempRoot();
        var useCases = CreateUseCases(
            unitOfWork,
            new FakeQuestionImportSessionLauncher(),
            new FakeQuestionImportDocumentCloseChecker { IsClosed = true });

        var session = await CreateReadySessionAsync(
            useCases,
            bankRootDirectory,
            sectionId,
            atomicSection.Id,
            AtomicSectionTeachingRole.Example,
            Difficulty.Basic,
            atomicSectionPanelId: panel.Id);

        var result = await useCases.ConfirmAsync(
            new ConfirmQuestionImportCommand(
                bankRootDirectory,
                session.SessionId,
                [
                    new ConfirmQuestionImportCandidateSelection(session.Candidates[0].CandidateId, Selected: true, Title: "Panel 题 1"),
                    new ConfirmQuestionImportCandidateSelection(session.Candidates[1].CandidateId, Selected: true, Title: "Panel 题 2")
                ]));

        var items = await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSection.Id);
        var panelItems = items
            .Where(item => item.AtomicSectionPanelId == panel.Id)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToArray();
        var unassignedItems = items.Where(item => item.AtomicSectionPanelId is null).ToArray();
        var importedBlocks = await Task.WhenAll(result.ContentBlockIds.Select(id => unitOfWork.ContentBlocks.GetByIdAsync(id)));

        Assert.Empty(result.SectionItemIds);
        Assert.Equal(2, result.AtomicSectionItemIds.Count);
        Assert.Equal(result.AtomicSectionItemIds[0], result.FirstInsertedNodeId);
        Assert.Equal("AtomicSectionItem", result.FirstInsertedNodeType);
        Assert.Equal([existingPanelItem.Id, .. result.AtomicSectionItemIds], panelItems.Select(item => item.Id));
        Assert.Equal([10, 20, 30], panelItems.Select(item => item.SortOrder));
        Assert.All(panelItems, item => Assert.Equal(AtomicSectionTeachingRole.Example, item.TeachingRole));
        Assert.Single(unassignedItems);
        Assert.Equal(existingUnassignedItem.Id, unassignedItems.Single().Id);
        Assert.All(importedBlocks, block => Assert.Equal(Difficulty.Basic, block!.Difficulty));
    }

    [Fact]
    public async Task CreateSession_rejects_question_import_into_knowledge_panel()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var atomicSection = new AtomicSection(sectionId, "AS");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();

        var knowledgePanel = new AtomicSectionPanel(
            atomicSection.Id,
            "知识点 panel",
            AtomicSectionTeachingRole.Knowledge,
            Difficulty.Basic,
            sortOrder: 10);
        await unitOfWork.AtomicSectionPanels.AddAsync(knowledgePanel);
        await unitOfWork.SaveChangesAsync();

        var bankRootDirectory = CreateTempRoot();
        var useCases = CreateUseCases(unitOfWork, new FakeQuestionImportSessionLauncher(), new FakeQuestionImportDocumentCloseChecker());

        var exception = await Assert.ThrowsAsync<CmsV2ApplicationException>(() => useCases.CreateSessionAsync(
            new CreateQuestionImportSessionCommand(
                bankRootDirectory,
                new InsertQuestionContext(
                    sectionId,
                    AtomicSectionId: atomicSection.Id,
                    AtomicSectionPanelId: knowledgePanel.Id,
                    AfterAtomicSectionItemId: null,
                    AfterSectionItemId: null,
                    DefaultTeachingRole: AtomicSectionTeachingRole.Example,
                    DefaultDifficulty: Difficulty.Advanced),
                OpenWord: false)));

        Assert.Contains("Knowledge panel", exception.Message);
        Assert.Empty(await unitOfWork.ContentBlocks.ListAsync());
        Assert.Empty(await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSection.Id));
    }

    [Fact]
    public async Task ConfirmImport_with_pre_class_quiz_panel_context_creates_normal_question_items()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var atomicSection = new AtomicSection(sectionId, "AS");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();

        var preClassQuizPanel = new AtomicSectionPanel(
            atomicSection.Id,
            "课前复习测验题",
            AtomicSectionTeachingRole.PreClassQuiz,
            Difficulty.Medium,
            sortOrder: 40);
        await unitOfWork.AtomicSectionPanels.AddAsync(preClassQuizPanel);
        await unitOfWork.SaveChangesAsync();

        var bankRootDirectory = CreateTempRoot();
        var useCases = CreateUseCases(
            unitOfWork,
            new FakeQuestionImportSessionLauncher(),
            new FakeQuestionImportDocumentCloseChecker { IsClosed = true });

        var session = await CreateReadySessionAsync(
            useCases,
            bankRootDirectory,
            sectionId,
            atomicSection.Id,
            AtomicSectionTeachingRole.Example,
            Difficulty.Advanced,
            atomicSectionPanelId: preClassQuizPanel.Id);

        var result = await useCases.ConfirmAsync(
            new ConfirmQuestionImportCommand(
                bankRootDirectory,
                session.SessionId,
                [new ConfirmQuestionImportCandidateSelection(session.Candidates[0].CandidateId, Selected: true, Title: "课前题")]));

        var item = (await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSection.Id)).Single();
        var block = await unitOfWork.ContentBlocks.GetByIdAsync(result.ContentBlockIds.Single());

        Assert.Empty(result.SectionItemIds);
        Assert.Single(result.AtomicSectionItemIds);
        Assert.Equal(preClassQuizPanel.Id, item.AtomicSectionPanelId);
        Assert.Equal(AtomicSectionTeachingRole.PreClassQuiz, item.TeachingRole);
        Assert.Equal(ContentBlockType.Question, block!.BlockType);
        Assert.Equal(QuestionType.Unset, block.QuestionType);
        Assert.Equal(Difficulty.Medium, block.Difficulty);
    }

    [Fact]
    public async Task CreateSession_rejects_panel_context_when_after_item_is_outside_that_panel()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var atomicSection = new AtomicSection(sectionId, "AS");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();

        var targetPanel = new AtomicSectionPanel(
            atomicSection.Id,
            "例题 panel",
            AtomicSectionTeachingRole.Example,
            Difficulty.Basic,
            sortOrder: 10);
        var otherPanel = new AtomicSectionPanel(
            atomicSection.Id,
            "练习 panel",
            AtomicSectionTeachingRole.Practice,
            Difficulty.Basic,
            sortOrder: 20);
        await unitOfWork.AtomicSectionPanels.AddAsync(targetPanel);
        await unitOfWork.AtomicSectionPanels.AddAsync(otherPanel);
        await unitOfWork.SaveChangesAsync();

        var block = await CreateContentBlockAsync(unitOfWork, sectionId, "其他 panel 题", Difficulty.Basic);
        var otherPanelItem = new AtomicSectionItem(
            atomicSection.Id,
            block.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 10,
            atomicSectionPanelId: otherPanel.Id,
            teachingRole: AtomicSectionTeachingRole.Practice);
        await unitOfWork.AtomicSectionItems.AddAsync(otherPanelItem);
        await unitOfWork.SaveChangesAsync();

        var bankRootDirectory = CreateTempRoot();
        var useCases = CreateUseCases(unitOfWork, new FakeQuestionImportSessionLauncher(), new FakeQuestionImportDocumentCloseChecker());

        var exception = await Assert.ThrowsAsync<CmsV2ApplicationException>(() => useCases.CreateSessionAsync(
            new CreateQuestionImportSessionCommand(
                bankRootDirectory,
                new InsertQuestionContext(
                    sectionId,
                    AtomicSectionId: atomicSection.Id,
                    AtomicSectionPanelId: targetPanel.Id,
                    AfterAtomicSectionItemId: otherPanelItem.Id,
                    AfterSectionItemId: null,
                    DefaultTeachingRole: targetPanel.TeachingRole,
                    DefaultDifficulty: targetPanel.Difficulty),
                OpenWord: false)));

        Assert.Contains("same AtomicSectionPanel", exception.Message);
    }

    [Fact]
    public async Task CancelSession_deletes_temporary_session_without_creating_formal_data()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var bankRootDirectory = CreateTempRoot();
        var useCases = CreateUseCases(unitOfWork, new FakeQuestionImportSessionLauncher(), new FakeQuestionImportDocumentCloseChecker());

        var session = await useCases.CreateSessionAsync(
            new CreateQuestionImportSessionCommand(
                bankRootDirectory,
                new InsertQuestionContext(sectionId, null, null, null, null, AtomicSectionTeachingRole.Unclassified, Difficulty.Basic),
                OpenWord: false));

        var cancelled = await useCases.CancelSessionAsync(
            new CancelQuestionImportSessionCommand(bankRootDirectory, session.SessionId));

        Assert.Equal(QuestionImportSessionStatus.Cancelled, cancelled.Status);
        Assert.False(Directory.Exists(Path.Combine(bankRootDirectory, "edit-sessions", "question-imports", session.SessionId)));
        Assert.Empty(await unitOfWork.ContentBlocks.ListAsync());
        Assert.Empty(await unitOfWork.ContentBlockVersions.ListAsync());
    }

    [Fact]
    public async Task ConfirmImport_failure_rolls_back_database_and_cleans_formal_files()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var bankRootDirectory = CreateTempRoot();
        var importProcessor = new FailingNeutralizeQuestionImportDocumentProcessor();
        var useCases = CreateUseCases(
            unitOfWork,
            new FakeQuestionImportSessionLauncher(),
            new FakeQuestionImportDocumentCloseChecker { IsClosed = true },
            importProcessor);
        var session = await CreateReadySessionAsync(useCases, bankRootDirectory, sectionId);

        await Assert.ThrowsAsync<InvalidOperationException>(() => useCases.ConfirmAsync(
            new ConfirmQuestionImportCommand(
                bankRootDirectory,
                session.SessionId,
                [new ConfirmQuestionImportCandidateSelection(session.Candidates[0].CandidateId, Selected: true, Title: "失败题目")])));

        Assert.Empty(await unitOfWork.ContentBlocks.ListAsync());
        Assert.Empty(await unitOfWork.ContentBlockVersions.ListAsync());
        Assert.Empty(await unitOfWork.ContentBlockVersionParts.ListAsync());
        Assert.Empty(await unitOfWork.SectionItems.ListBySectionAsync(sectionId));
        Assert.False(File.Exists(Path.Combine(bankRootDirectory, "content-blocks", "source", "1", "v1.docx")));
        Assert.True(Directory.Exists(Path.Combine(bankRootDirectory, "edit-sessions", "question-imports", session.SessionId)));
    }

    private static async Task<QuestionImportSessionDto> CreateReadySessionAsync(
        QuestionImportUseCases useCases,
        string bankRootDirectory,
        int sectionId,
        int? atomicSectionId = null,
        AtomicSectionTeachingRole defaultTeachingRole = AtomicSectionTeachingRole.Unclassified,
        Difficulty defaultDifficulty = Difficulty.Medium,
        int? atomicSectionPanelId = null,
        int? afterAtomicSectionItemId = null,
        int? afterSectionItemId = null)
    {
        var session = await useCases.CreateSessionAsync(
            new CreateQuestionImportSessionCommand(
                bankRootDirectory,
                new InsertQuestionContext(
                    sectionId,
                    atomicSectionId,
                    atomicSectionPanelId,
                    afterAtomicSectionItemId,
                    afterSectionItemId,
                    defaultTeachingRole,
                    defaultDifficulty),
                OpenWord: false));
        CreateMultiQuestionDocx(Path.Combine(bankRootDirectory, "edit-sessions", "question-imports", session.SessionId, "source.docx"));
        return await useCases.GetSessionAsync(new GetQuestionImportSessionCommand(bankRootDirectory, session.SessionId));
    }

    private static QuestionImportUseCases CreateUseCases(
        EfCmsV2UnitOfWork unitOfWork,
        IQuestionImportSessionLauncher launcher,
        IQuestionImportDocumentCloseChecker closeChecker,
        IQuestionImportDocumentProcessor? questionImportProcessor = null)
    {
        return new QuestionImportUseCases(
            unitOfWork,
            new CmsV2FileAssetPathProvider(),
            new LocalContentBlockFileStore(),
            new AsposeContentBlockDocumentProcessor(),
            questionImportProcessor ?? new AsposeQuestionImportDocumentProcessor(),
            launcher,
            closeChecker);
    }

    private static async Task<int> CreateSectionAsync(
        EfCmsV2UnitOfWork unitOfWork,
        string topicName = "测试主题",
        string sectionTitle = "测试 Section")
    {
        var topic = new TeachingTopic(topicName);
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        var section = new DomainSection(topic.Id, sectionTitle);
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();

        return section.Id;
    }

    private static async Task<ContentBlock> CreateContentBlockAsync(
        EfCmsV2UnitOfWork unitOfWork,
        int sectionId,
        string title,
        Difficulty difficulty)
    {
        var block = new ContentBlock(
            sectionId,
            title,
            ContentBlockType.Question,
            difficulty: difficulty,
            questionType: QuestionType.Unset,
            status: ContentBlockStatus.Active);
        await unitOfWork.ContentBlocks.AddAsync(block);
        await unitOfWork.SaveChangesAsync();

        return block;
    }

    private static async Task<CmsV2DbContext> CreateMigratedContextAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-question-import-tests",
            Guid.NewGuid().ToString("N"),
            "cms-v2.db");

        var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        return context;
    }

    private static string CreateTempRoot()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "cms-v2-question-import-tests",
            Guid.NewGuid().ToString("N"));
    }

    private static void CreateMultiQuestionDocx(string docxPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);

        var document = new Document();
        var body = document.FirstSection.Body;
        body.RemoveAllChildren();

        AddStyledParagraph(document, body, "正文", "导入前说明，应该被丢弃。");
        AddStyledParagraph(document, body, "例题", "第一题题干");
        AddStyledParagraph(document, body, "答案", "第一题答案");
        AddStyledParagraph(document, body, "典型例题", "典型例题题干");
        AddStyledParagraph(document, body, "解析", "典型例题解析");
        AddStyledParagraph(document, body, "练习题", "练习题题干");
        AddStyledParagraph(document, body, "答案", "练习题答案");

        document.Save(docxPath);
    }

    private static void AddStyledParagraph(Document document, Body body, string styleName, string text)
    {
        EnsureParagraphStyle(document, styleName);
        var paragraph = new Paragraph(document);
        paragraph.ParagraphFormat.StyleName = styleName;
        paragraph.AppendChild(new Run(document, text));
        body.AppendChild(paragraph);
    }

    private static void EnsureParagraphStyle(Document document, string styleName)
    {
        if (document.Styles[styleName] is not null)
        {
            return;
        }

        document.Styles.Add(StyleType.Paragraph, styleName);
    }

    private sealed class FakeQuestionImportSessionLauncher : IQuestionImportSessionLauncher
    {
        public List<string> OpenedDocxPaths { get; } = [];

        public Task OpenAsync(QuestionImportSessionLaunchRequest request, CancellationToken cancellationToken = default)
        {
            OpenedDocxPaths.Add(request.SourceDocxPath);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeQuestionImportDocumentCloseChecker : IQuestionImportDocumentCloseChecker
    {
        public bool IsClosed { get; init; }

        public Task<bool> IsClosedAsync(string sourceDocxPath, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(IsClosed);
        }
    }

    private sealed class FailingNeutralizeQuestionImportDocumentProcessor : IQuestionImportDocumentProcessor
    {
        private readonly AsposeQuestionImportDocumentProcessor _inner = new();

        public Task<IReadOnlyList<QuestionImportCandidateDocumentResult>> SplitCandidatesAsync(
            string sourceDocxPath,
            string candidateDirectory,
            CancellationToken cancellationToken = default)
        {
            return _inner.SplitCandidatesAsync(sourceDocxPath, candidateDirectory, cancellationToken);
        }

        public Task CreateNeutralizedCandidateDocxAsync(
            string candidateDocxPath,
            string outputDocxPath,
            CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputDocxPath)!);
            File.Copy(candidateDocxPath, outputDocxPath, overwrite: true);
            throw new InvalidOperationException("Simulated formal docx failure.");
        }
    }
}
