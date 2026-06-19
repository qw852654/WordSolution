using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Application.ContentBlocks;

public sealed class ContentBlockEditSessionUseCases
{
    private readonly ICmsV2UnitOfWork _unitOfWork;
    private readonly IContentBlockFileStore _contentBlockFileStore;
    private readonly IContentBlockEditSessionStore _sessionStore;
    private readonly IContentBlockEditSessionFileStore _sessionFileStore;
    private readonly IContentBlockEditSessionLauncher _launcher;
    private readonly ContentBlockDocumentUseCases _documentUseCases;

    public ContentBlockEditSessionUseCases(
        ICmsV2UnitOfWork unitOfWork,
        IContentBlockFileStore contentBlockFileStore,
        IContentBlockEditSessionStore sessionStore,
        IContentBlockEditSessionFileStore sessionFileStore,
        IContentBlockEditSessionLauncher launcher,
        ContentBlockDocumentUseCases documentUseCases)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _contentBlockFileStore = contentBlockFileStore ?? throw new ArgumentNullException(nameof(contentBlockFileStore));
        _sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
        _sessionFileStore = sessionFileStore ?? throw new ArgumentNullException(nameof(sessionFileStore));
        _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
        _documentUseCases = documentUseCases ?? throw new ArgumentNullException(nameof(documentUseCases));
    }

    public async Task<ContentBlockEditSession> CreateAsync(
        CreateContentBlockEditSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(command.BankRootDirectory);
        var contentBlock = await RequireContentBlockAsync(command.ContentBlockId, cancellationToken);
        var currentVersion = await EnsureCurrentVersionAsync(command.BankRootDirectory, contentBlock, cancellationToken);
        var sourceDocx = await _contentBlockFileStore.ReadContentBlockDocxAsync(
            currentVersion.DocxPath,
            cancellationToken);

        if (sourceDocx is null)
        {
            throw new CmsV2ApplicationException($"ContentBlockVersion DOCX file was not found: {currentVersion.DocxPath}");
        }

        var sessionId = Guid.NewGuid().ToString("N");
        var editableDocxPath = await _sessionFileStore.CreateEditableDocxAsync(
            command.BankRootDirectory,
            sessionId,
            sourceDocx,
            cancellationToken);
        var originalHash = await _sessionFileStore.ComputeHashAsync(editableDocxPath, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var session = new ContentBlockEditSession(
            sessionId,
            contentBlock.Id,
            currentVersion.Id,
            editableDocxPath,
            originalHash,
            ContentBlockEditSessionStatus.Created,
            ContentBlockEditLaunchMode.None,
            OpenedByServer: false,
            Message: null,
            now,
            now);

        if (command.OpenWord)
        {
            session = session with
            {
                Status = ContentBlockEditSessionStatus.Opening,
                UpdatedTime = DateTimeOffset.UtcNow
            };
            await _sessionStore.SaveAsync(command.BankRootDirectory, session, cancellationToken);

            var launchResult = await _launcher.LaunchAsync(session, cancellationToken);
            session = session with
            {
                Status = launchResult.OpenedByServer
                    ? ContentBlockEditSessionStatus.Editing
                    : ContentBlockEditSessionStatus.Created,
                LaunchMode = launchResult.LaunchMode,
                OpenedByServer = launchResult.OpenedByServer,
                Message = launchResult.Message,
                UpdatedTime = DateTimeOffset.UtcNow
            };
        }

        await _sessionStore.SaveAsync(command.BankRootDirectory, session, cancellationToken);

        return session;
    }

    public async Task<ContentBlockEditSession?> GetAsync(
        GetContentBlockEditSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(command.BankRootDirectory);
        ValidateSessionId(command.SessionId);

        return await _sessionStore.GetAsync(command.BankRootDirectory, command.SessionId, cancellationToken);
    }

    public async Task<SyncContentBlockEditSessionResult> SyncAsync(
        SyncContentBlockEditSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var session = await RequireSessionAsync(command.BankRootDirectory, command.SessionId, cancellationToken);

        if (session.Status is ContentBlockEditSessionStatus.Cancelled or ContentBlockEditSessionStatus.Synced)
        {
            throw new CmsV2ApplicationException($"ContentBlockEditSession {session.SessionId} cannot be synced from status {session.Status}.");
        }

        var currentHash = await _sessionFileStore.ComputeHashAsync(session.EditableDocxPath, cancellationToken);
        if (string.Equals(currentHash, session.OriginalDocxHash, StringComparison.OrdinalIgnoreCase))
        {
            var sourceVersion = await RequireContentBlockVersionAsync(session.SourceContentBlockVersionId, cancellationToken);
            var syncedSession = session with
            {
                Status = ContentBlockEditSessionStatus.Synced,
                Message = "No changes were detected.",
                UpdatedTime = DateTimeOffset.UtcNow
            };
            await _sessionStore.SaveAsync(command.BankRootDirectory, syncedSession, cancellationToken);

            return new SyncContentBlockEditSessionResult(
                syncedSession.SessionId,
                syncedSession.ContentBlockId,
                Changed: false,
                NewContentBlockVersionId: null,
                sourceVersion.VersionNumber,
                syncedSession.Status,
                syncedSession.Message);
        }

        var editedDocx = await _sessionFileStore.ReadEditableDocxAsync(session.EditableDocxPath, cancellationToken);
        if (editedDocx is null)
        {
            throw new CmsV2ApplicationException($"ContentBlock edit session DOCX file was not found: {session.EditableDocxPath}");
        }

        await using var stream = new MemoryStream(editedDocx);
        var imported = await _documentUseCases.ImportContentBlockDocxVersionAsync(
            new ImportContentBlockDocxVersionCommand(
                command.BankRootDirectory,
                session.ContentBlockId,
                stream,
                SetAsCurrent: true),
            cancellationToken);
        var changedSession = session with
        {
            Status = ContentBlockEditSessionStatus.Synced,
            Message = "Changes were synced.",
            UpdatedTime = DateTimeOffset.UtcNow
        };
        await _sessionStore.SaveAsync(command.BankRootDirectory, changedSession, cancellationToken);

        return new SyncContentBlockEditSessionResult(
            changedSession.SessionId,
            changedSession.ContentBlockId,
            Changed: true,
            imported.ContentBlockVersionId,
            imported.VersionNumber,
            changedSession.Status,
            changedSession.Message);
    }

    public async Task<IReadOnlyList<SyncContentBlockEditSessionResult>> SyncActiveSessionsAsync(
        SyncActiveContentBlockEditSessionsCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(command.BankRootDirectory);
        if (command.MinimumSessionAge < TimeSpan.Zero)
        {
            throw new CmsV2ApplicationException("MinimumSessionAge cannot be negative.");
        }

        var now = DateTimeOffset.UtcNow;
        var activeSessions = await _sessionStore.ListActiveAsync(command.BankRootDirectory, cancellationToken);
        var results = new List<SyncContentBlockEditSessionResult>();

        foreach (var session in activeSessions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (now - session.CreatedTime < command.MinimumSessionAge)
            {
                continue;
            }

            var available = await _sessionFileStore.IsEditableDocxAvailableForSyncAsync(
                session.EditableDocxPath,
                cancellationToken);
            if (!available)
            {
                continue;
            }

            var result = await SyncAsync(
                new SyncContentBlockEditSessionCommand(command.BankRootDirectory, session.SessionId),
                cancellationToken);
            results.Add(result);
        }

        return results;
    }

    public async Task<ContentBlockEditSession> CancelAsync(
        CancelContentBlockEditSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var session = await RequireSessionAsync(command.BankRootDirectory, command.SessionId, cancellationToken);

        if (session.Status == ContentBlockEditSessionStatus.Synced)
        {
            throw new CmsV2ApplicationException($"ContentBlockEditSession {session.SessionId} has already been synced.");
        }

        var cancelled = session with
        {
            Status = ContentBlockEditSessionStatus.Cancelled,
            Message = "Session was cancelled.",
            UpdatedTime = DateTimeOffset.UtcNow
        };
        await _sessionStore.SaveAsync(command.BankRootDirectory, cancelled, cancellationToken);

        return cancelled;
    }

    private async Task<ContentBlock> RequireContentBlockAsync(int contentBlockId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ContentBlocks.GetByIdAsync(contentBlockId, cancellationToken)
            ?? throw new CmsV2ApplicationException($"ContentBlock {contentBlockId} was not found.");
    }

    private async Task<ContentBlockVersion> EnsureCurrentVersionAsync(
        string bankRootDirectory,
        ContentBlock contentBlock,
        CancellationToken cancellationToken)
    {
        if (!contentBlock.CurrentVersionId.HasValue)
        {
            var result = await _documentUseCases.CreateBlankContentBlockVersionAsync(
                new CreateBlankContentBlockVersionCommand(
                    bankRootDirectory,
                    contentBlock.Id,
                    SetAsCurrent: true),
                cancellationToken);

            return await _unitOfWork.ContentBlockVersions.GetByIdAsync(result.ContentBlockVersionId, cancellationToken)
                ?? throw new CmsV2ApplicationException($"ContentBlockVersion {result.ContentBlockVersionId} was not found.");
        }

        return await _unitOfWork.ContentBlockVersions.GetByIdAsync(contentBlock.CurrentVersionId.Value, cancellationToken)
            ?? throw new CmsV2ApplicationException($"ContentBlockVersion {contentBlock.CurrentVersionId.Value} was not found.");
    }

    private async Task<ContentBlockVersion> RequireContentBlockVersionAsync(
        int contentBlockVersionId,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.ContentBlockVersions.GetByIdAsync(contentBlockVersionId, cancellationToken)
            ?? throw new CmsV2ApplicationException($"ContentBlockVersion {contentBlockVersionId} was not found.");
    }

    private async Task<ContentBlockEditSession> RequireSessionAsync(
        string bankRootDirectory,
        string sessionId,
        CancellationToken cancellationToken)
    {
        ValidateBankRootDirectory(bankRootDirectory);
        ValidateSessionId(sessionId);

        return await _sessionStore.GetAsync(bankRootDirectory, sessionId, cancellationToken)
            ?? throw new CmsV2ApplicationException($"ContentBlockEditSession {sessionId} was not found.");
    }

    private static void ValidateBankRootDirectory(string bankRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(bankRootDirectory))
        {
            throw new CmsV2ApplicationException("BankRootDirectory cannot be empty.");
        }
    }

    private static void ValidateSessionId(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new CmsV2ApplicationException("SessionId cannot be empty.");
        }
    }
}
