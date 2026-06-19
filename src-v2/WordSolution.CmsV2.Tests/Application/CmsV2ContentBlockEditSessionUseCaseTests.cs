using System.IO.Compression;
using System.Security;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Application.ContentBlocks;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Infrastructure.Documents;
using WordSolution.CmsV2.Infrastructure.Persistence;
using WordSolution.CmsV2.Infrastructure.Repositories;

namespace WordSolution.CmsV2.Tests.Application;

public sealed class CmsV2ContentBlockEditSessionUseCaseTests
{
    [Fact]
    public void Session_store_contract_exposes_active_session_listing()
    {
        var method = typeof(IContentBlockEditSessionStore).GetMethod("ListActiveAsync");

        Assert.NotNull(method);
    }

    [Fact]
    public async Task Session_store_lists_only_active_sessions()
    {
        var bankRootDirectory = CreateTempRoot();
        var store = new LocalContentBlockEditSessionStore();
        var now = DateTimeOffset.UtcNow;
        var active = CreateSession("active", ContentBlockEditSessionStatus.Editing, now.AddMinutes(-2));
        var synced = CreateSession("synced", ContentBlockEditSessionStatus.Synced, now.AddMinutes(-1));
        var cancelled = CreateSession("cancelled", ContentBlockEditSessionStatus.Cancelled, now);
        await store.SaveAsync(bankRootDirectory, synced);
        await store.SaveAsync(bankRootDirectory, active);
        await store.SaveAsync(bankRootDirectory, cancelled);

        var sessions = await store.ListActiveAsync(bankRootDirectory);

        var session = Assert.Single(sessions);
        Assert.Equal(active.SessionId, session.SessionId);
    }

    [Fact]
    public async Task Sync_active_sessions_imports_changed_edit_session_without_manual_sync()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var documentUseCases = CreateDocumentUseCases(unitOfWork);
        var sessionStore = new LocalContentBlockEditSessionStore();
        var useCases = CreateEditSessionUseCases(
            unitOfWork,
            documentUseCases,
            sessionStore,
            new LocalContentBlockEditSessionFileStore(),
            new RecordingContentBlockEditSessionLauncher());
        var contentBlockId = await CreateImportedContentBlockAsync(unitOfWork, documentUseCases, bankRootDirectory, "原始正文");
        var session = await useCases.CreateAsync(
            new CreateContentBlockEditSessionCommand(bankRootDirectory, contentBlockId, OpenWord: false));
        var stored = await sessionStore.GetAsync(bankRootDirectory, session.SessionId)
            ?? throw new InvalidOperationException("The test session was not persisted.");
        await CreateMinimalDocxAsync(stored.EditableDocxPath, "后台自动同步后的正文");

        var results = await useCases.SyncActiveSessionsAsync(
            new SyncActiveContentBlockEditSessionsCommand(bankRootDirectory, MinimumSessionAge: TimeSpan.Zero));
        var currentVersion = await unitOfWork.ContentBlockVersions.GetCurrentByContentBlockAsync(contentBlockId);

        var result = Assert.Single(results);
        Assert.True(result.Changed);
        Assert.NotNull(currentVersion);
        Assert.Equal(result.NewContentBlockVersionId, currentVersion.Id);
        Assert.Contains("后台自动同步后的正文", currentVersion.PlainText);
    }

    [Fact]
    public async Task Create_session_copies_current_docx_and_invokes_launcher_when_requested()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var documentUseCases = CreateDocumentUseCases(unitOfWork);
        var launcher = new RecordingContentBlockEditSessionLauncher();
        var sessionStore = new LocalContentBlockEditSessionStore();
        var sessionFileStore = new LocalContentBlockEditSessionFileStore();
        var useCases = CreateEditSessionUseCases(unitOfWork, documentUseCases, sessionStore, sessionFileStore, launcher);
        var contentBlockId = await CreateImportedContentBlockAsync(unitOfWork, documentUseCases, bankRootDirectory, "原始正文");

        var session = await useCases.CreateAsync(
            new CreateContentBlockEditSessionCommand(bankRootDirectory, contentBlockId, OpenWord: true));
        var stored = await sessionStore.GetAsync(bankRootDirectory, session.SessionId);

        Assert.Equal(contentBlockId, session.ContentBlockId);
        Assert.Equal(ContentBlockEditSessionStatus.Editing, session.Status);
        Assert.Equal(ContentBlockEditLaunchMode.LocalShell, session.LaunchMode);
        Assert.True(session.OpenedByServer);
        Assert.NotNull(stored);
        Assert.True(File.Exists(stored.EditableDocxPath));
        Assert.Equal(stored.EditableDocxPath, launcher.LaunchedFilePath);
        Assert.Equal(1, launcher.LaunchCount);
    }

    [Fact]
    public async Task Create_session_creates_initial_template_version_when_content_block_has_no_current_version()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var documentUseCases = CreateDocumentUseCases(unitOfWork);
        var sessionStore = new LocalContentBlockEditSessionStore();
        var sessionFileStore = new LocalContentBlockEditSessionFileStore();
        var useCases = CreateEditSessionUseCases(
            unitOfWork,
            documentUseCases,
            sessionStore,
            sessionFileStore,
            new RecordingContentBlockEditSessionLauncher());
        var topic = new WordSolution.CmsV2.Domain.Entities.TeachingTopic("Topic");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();
        var section = new WordSolution.CmsV2.Domain.Entities.Section(topic.Id, "Section");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();
        var contentBlock = new WordSolution.CmsV2.Domain.Entities.ContentBlock(
            section.Id,
            string.Empty,
            ContentBlockType.KnowledgePoint,
            difficulty: Difficulty.Basic);
        await unitOfWork.ContentBlocks.AddAsync(contentBlock);
        await unitOfWork.SaveChangesAsync();

        var session = await useCases.CreateAsync(
            new CreateContentBlockEditSessionCommand(bankRootDirectory, contentBlock.Id, OpenWord: false));
        var versions = await unitOfWork.ContentBlockVersions.ListByContentBlockAsync(contentBlock.Id);
        var currentVersion = await unitOfWork.ContentBlockVersions.GetCurrentByContentBlockAsync(contentBlock.Id);
        var reloadedBlock = await unitOfWork.ContentBlocks.GetByIdAsync(contentBlock.Id);

        var version = Assert.Single(versions);
        Assert.NotNull(currentVersion);
        Assert.Equal(currentVersion.Id, version.Id);
        Assert.Equal(version.Id, reloadedBlock?.CurrentVersionId);
        Assert.Equal(1, version.VersionNumber);
        Assert.True(version.IsCurrent);
        Assert.Equal(version.Id, session.SourceContentBlockVersionId);
        Assert.True(File.Exists(version.DocxPath));
        Assert.True(File.Exists(version.HtmlPreviewPath));
        Assert.True(File.Exists(session.EditableDocxPath));
    }

    [Fact]
    public async Task Sync_unchanged_session_does_not_create_new_version()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var documentUseCases = CreateDocumentUseCases(unitOfWork);
        var sessionStore = new LocalContentBlockEditSessionStore();
        var useCases = CreateEditSessionUseCases(
            unitOfWork,
            documentUseCases,
            sessionStore,
            new LocalContentBlockEditSessionFileStore(),
            new RecordingContentBlockEditSessionLauncher());
        var contentBlockId = await CreateImportedContentBlockAsync(unitOfWork, documentUseCases, bankRootDirectory, "原始正文");
        var session = await useCases.CreateAsync(
            new CreateContentBlockEditSessionCommand(bankRootDirectory, contentBlockId, OpenWord: false));

        var result = await useCases.SyncAsync(
            new SyncContentBlockEditSessionCommand(bankRootDirectory, session.SessionId));
        var versions = await unitOfWork.ContentBlockVersions.ListByContentBlockAsync(contentBlockId);

        Assert.False(result.Changed);
        Assert.Null(result.NewContentBlockVersionId);
        Assert.Equal(ContentBlockEditSessionStatus.Synced, result.Status);
        Assert.Equal(2, result.CurrentVersionNumber);
        Assert.Equal(2, versions.Count);
    }

    [Fact]
    public async Task Sync_changed_session_imports_new_current_version()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var documentUseCases = CreateDocumentUseCases(unitOfWork);
        var sessionStore = new LocalContentBlockEditSessionStore();
        var useCases = CreateEditSessionUseCases(
            unitOfWork,
            documentUseCases,
            sessionStore,
            new LocalContentBlockEditSessionFileStore(),
            new RecordingContentBlockEditSessionLauncher());
        var contentBlockId = await CreateImportedContentBlockAsync(unitOfWork, documentUseCases, bankRootDirectory, "原始正文");
        var session = await useCases.CreateAsync(
            new CreateContentBlockEditSessionCommand(bankRootDirectory, contentBlockId, OpenWord: false));
        var stored = await sessionStore.GetAsync(bankRootDirectory, session.SessionId)
            ?? throw new InvalidOperationException("The test session was not persisted.");
        await CreateMinimalDocxAsync(stored.EditableDocxPath, "编辑后的正文");

        var result = await useCases.SyncAsync(
            new SyncContentBlockEditSessionCommand(bankRootDirectory, session.SessionId));
        var versions = await unitOfWork.ContentBlockVersions.ListByContentBlockAsync(contentBlockId);
        var currentVersion = await unitOfWork.ContentBlockVersions.GetCurrentByContentBlockAsync(contentBlockId);

        Assert.True(result.Changed);
        Assert.NotNull(result.NewContentBlockVersionId);
        Assert.Equal(3, result.CurrentVersionNumber);
        Assert.Equal(3, versions.Count);
        Assert.NotNull(currentVersion);
        Assert.Equal(result.NewContentBlockVersionId, currentVersion.Id);
        Assert.Contains("编辑后的正文", currentVersion.PlainText);
    }

    [Fact]
    public async Task Cancel_marks_session_cancelled()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var documentUseCases = CreateDocumentUseCases(unitOfWork);
        var useCases = CreateEditSessionUseCases(
            unitOfWork,
            documentUseCases,
            new LocalContentBlockEditSessionStore(),
            new LocalContentBlockEditSessionFileStore(),
            new RecordingContentBlockEditSessionLauncher());
        var contentBlockId = await CreateImportedContentBlockAsync(unitOfWork, documentUseCases, bankRootDirectory, "原始正文");
        var session = await useCases.CreateAsync(
            new CreateContentBlockEditSessionCommand(bankRootDirectory, contentBlockId, OpenWord: false));

        var cancelled = await useCases.CancelAsync(
            new CancelContentBlockEditSessionCommand(bankRootDirectory, session.SessionId));

        Assert.Equal(ContentBlockEditSessionStatus.Cancelled, cancelled.Status);
    }

    private static ContentBlockEditSessionUseCases CreateEditSessionUseCases(
        EfCmsV2UnitOfWork unitOfWork,
        ContentBlockDocumentUseCases documentUseCases,
        IContentBlockEditSessionStore sessionStore,
        IContentBlockEditSessionFileStore sessionFileStore,
        IContentBlockEditSessionLauncher launcher)
    {
        return new ContentBlockEditSessionUseCases(
            unitOfWork,
            new LocalContentBlockFileStore(),
            sessionStore,
            sessionFileStore,
            launcher,
            documentUseCases);
    }

    private static ContentBlockDocumentUseCases CreateDocumentUseCases(EfCmsV2UnitOfWork unitOfWork)
    {
        return new ContentBlockDocumentUseCases(
            unitOfWork,
            new CmsV2FileAssetPathProvider(),
            new LocalContentBlockFileStore(),
            new AsposeContentBlockDocumentProcessor());
    }

    private static async Task<int> CreateImportedContentBlockAsync(
        EfCmsV2UnitOfWork unitOfWork,
        ContentBlockDocumentUseCases documentUseCases,
        string bankRootDirectory,
        string text)
    {
        var topic = new WordSolution.CmsV2.Domain.Entities.TeachingTopic("测试主题");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        var section = new WordSolution.CmsV2.Domain.Entities.Section(topic.Id, "测试 Section");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();

        var created = await documentUseCases.CreateContentBlockWithBlankDocumentAsync(
            new CreateContentBlockWithBlankDocumentCommand(
                bankRootDirectory,
                section.Id,
                "测试 ContentBlock",
                ContentBlockType.KnowledgePoint));
        var importDocxPath = Path.Combine(bankRootDirectory, "imports", $"{Guid.NewGuid():N}.docx");
        await CreateMinimalDocxAsync(importDocxPath, text);

        await using var importStream = File.OpenRead(importDocxPath);
        await documentUseCases.ImportContentBlockDocxVersionAsync(
            new ImportContentBlockDocxVersionCommand(
                bankRootDirectory,
                created.ContentBlockId,
                importStream,
                SetAsCurrent: true));

        return created.ContentBlockId;
    }

    private static async Task<CmsV2DbContext> CreateMigratedContextAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-edit-session-use-case-tests",
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
            "cms-v2-edit-session-use-case-tests",
            Guid.NewGuid().ToString("N"));
    }

    private static ContentBlockEditSession CreateSession(
        string sessionId,
        ContentBlockEditSessionStatus status,
        DateTimeOffset createdTime)
    {
        return new ContentBlockEditSession(
            sessionId,
            ContentBlockId: 1,
            SourceContentBlockVersionId: 1,
            EditableDocxPath: Path.Combine(Path.GetTempPath(), sessionId, "edit.docx"),
            OriginalDocxHash: "hash",
            status,
            ContentBlockEditLaunchMode.None,
            OpenedByServer: false,
            Message: null,
            createdTime,
            createdTime);
    }

    private static async Task CreateMinimalDocxAsync(string docxPath, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);

        if (File.Exists(docxPath))
        {
            File.Delete(docxPath);
        }

        using var archive = ZipFile.Open(docxPath, ZipArchiveMode.Create);
        await WriteEntryAsync(
            archive,
            "[Content_Types].xml",
            """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
              <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
              <Default Extension="xml" ContentType="application/xml"/>
              <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
            </Types>
            """);
        await WriteEntryAsync(
            archive,
            "_rels/.rels",
            """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
            </Relationships>
            """);
        await WriteEntryAsync(
            archive,
            "word/document.xml",
            $$"""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
              <w:body>
                <w:p>
                  <w:r>
                    <w:t>{{SecurityElement.Escape(text)}}</w:t>
                  </w:r>
                </w:p>
                <w:sectPr/>
              </w:body>
            </w:document>
            """);
    }

    private static async Task WriteEntryAsync(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName);
        await using var stream = entry.Open();
        await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        await writer.WriteAsync(content);
    }

    private sealed class RecordingContentBlockEditSessionLauncher : IContentBlockEditSessionLauncher
    {
        public int LaunchCount { get; private set; }
        public string? LaunchedFilePath { get; private set; }

        public Task<ContentBlockEditLaunchResult> LaunchAsync(
            ContentBlockEditSession session,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LaunchCount++;
            LaunchedFilePath = session.EditableDocxPath;

            return Task.FromResult(new ContentBlockEditLaunchResult(
                ContentBlockEditLaunchMode.LocalShell,
                OpenedByServer: true,
                Message: "Launched by test."));
        }
    }
}
