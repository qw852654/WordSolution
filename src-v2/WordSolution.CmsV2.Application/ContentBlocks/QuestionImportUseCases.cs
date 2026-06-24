using System.Text.Json;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Application.ContentBlocks;

public sealed class QuestionImportUseCases
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ICmsV2UnitOfWork _unitOfWork;
    private readonly ICmsV2FileAssetPathProvider _pathProvider;
    private readonly IContentBlockFileStore _fileStore;
    private readonly IContentBlockDocumentProcessor _documentProcessor;
    private readonly IQuestionImportDocumentProcessor _questionImportProcessor;
    private readonly IQuestionImportSessionLauncher _sessionLauncher;
    private readonly IQuestionImportDocumentCloseChecker _closeChecker;

    public QuestionImportUseCases(
        ICmsV2UnitOfWork unitOfWork,
        ICmsV2FileAssetPathProvider pathProvider,
        IContentBlockFileStore fileStore,
        IContentBlockDocumentProcessor documentProcessor,
        IQuestionImportDocumentProcessor questionImportProcessor,
        IQuestionImportSessionLauncher sessionLauncher,
        IQuestionImportDocumentCloseChecker closeChecker)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
        _documentProcessor = documentProcessor ?? throw new ArgumentNullException(nameof(documentProcessor));
        _questionImportProcessor = questionImportProcessor ?? throw new ArgumentNullException(nameof(questionImportProcessor));
        _sessionLauncher = sessionLauncher ?? throw new ArgumentNullException(nameof(sessionLauncher));
        _closeChecker = closeChecker ?? throw new ArgumentNullException(nameof(closeChecker));
    }

    public async Task<QuestionImportSessionDto> CreateSessionAsync(
        CreateQuestionImportSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(command.BankRootDirectory);
        var context = await ValidateContextAsync(command.Context, cancellationToken);

        var sessionId = Guid.NewGuid().ToString("N");
        var sessionDirectory = GetSessionDirectory(command.BankRootDirectory, sessionId);
        var sourceDocxPath = GetSourceDocxPath(command.BankRootDirectory, sessionId);
        Directory.CreateDirectory(sessionDirectory);
        await _documentProcessor.CreateBlankDocxAsync(sourceDocxPath, cancellationToken);

        var record = new QuestionImportSessionRecord(
            sessionId,
            context,
            QuestionImportSessionStatus.Created,
            Message: null,
            CreatedTime: DateTimeOffset.UtcNow,
            UpdatedTime: DateTimeOffset.UtcNow,
            SourceDocxPath: sourceDocxPath,
            Candidates: []);
        await SaveSessionRecordAsync(command.BankRootDirectory, record, cancellationToken);

        if (command.OpenWord)
        {
            record = record with
            {
                Status = QuestionImportSessionStatus.Opening,
                UpdatedTime = DateTimeOffset.UtcNow
            };
            await SaveSessionRecordAsync(command.BankRootDirectory, record, cancellationToken);
            await _sessionLauncher.OpenAsync(
                new QuestionImportSessionLaunchRequest(record.SessionId, record.SourceDocxPath),
                cancellationToken);
            record = record with
            {
                Status = QuestionImportSessionStatus.Editing,
                UpdatedTime = DateTimeOffset.UtcNow
            };
            await SaveSessionRecordAsync(command.BankRootDirectory, record, cancellationToken);
        }

        return await ToDtoAsync(record, cancellationToken);
    }

    public async Task<QuestionImportSessionDto> GetSessionAsync(
        GetQuestionImportSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(command.BankRootDirectory);
        var record = await LoadSessionRecordAsync(command.BankRootDirectory, command.SessionId, cancellationToken);

        if (record.Status is QuestionImportSessionStatus.Created or QuestionImportSessionStatus.Editing
            && await _closeChecker.IsClosedAsync(record.SourceDocxPath, cancellationToken))
        {
            record = await ParseSessionAsync(command.BankRootDirectory, record, cancellationToken);
        }

        return await ToDtoAsync(record, cancellationToken);
    }

    public async Task<IReadOnlyList<QuestionImportCandidateDto>> GetCandidatesAsync(
        GetQuestionImportSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync(command, cancellationToken);
        return session.Candidates;
    }

    public async Task<QuestionImportSessionDto> ReopenSessionAsync(
        ReopenQuestionImportSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(command.BankRootDirectory);
        var record = await LoadSessionRecordAsync(command.BankRootDirectory, command.SessionId, cancellationToken);
        EnsureSessionCanReopen(record);

        record = record with
        {
            Status = QuestionImportSessionStatus.Opening,
            UpdatedTime = DateTimeOffset.UtcNow
        };
        await SaveSessionRecordAsync(command.BankRootDirectory, record, cancellationToken);
        await _sessionLauncher.OpenAsync(
            new QuestionImportSessionLaunchRequest(record.SessionId, record.SourceDocxPath),
            cancellationToken);
        record = record with
        {
            Status = QuestionImportSessionStatus.Editing,
            UpdatedTime = DateTimeOffset.UtcNow
        };
        await SaveSessionRecordAsync(command.BankRootDirectory, record, cancellationToken);

        return await ToDtoAsync(record, cancellationToken);
    }

    public async Task<QuestionImportConfirmResult> ConfirmAsync(
        ConfirmQuestionImportCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(command.BankRootDirectory);
        var record = await LoadSessionRecordAsync(command.BankRootDirectory, command.SessionId, cancellationToken);
        if (record.Status != QuestionImportSessionStatus.ReadyForReview)
        {
            throw new CmsV2ApplicationException("Question import session is not ready for review.");
        }

        var context = await ValidateContextAsync(record.Context, cancellationToken);
        record = record with { Context = context };
        var selectedCandidates = ResolveSelectedCandidates(record, command.Candidates);
        var generatedFilePaths = new List<string>();

        record = record with
        {
            Status = QuestionImportSessionStatus.Importing,
            UpdatedTime = DateTimeOffset.UtcNow
        };
        await SaveSessionRecordAsync(command.BankRootDirectory, record, cancellationToken);

        try
        {
            var result = await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
            {
                var contentBlockIds = new List<int>();
                var contentBlockVersionIds = new List<int>();
                var sectionItems = new List<SectionItem>();
                var atomicSectionItems = new List<AtomicSectionItem>();

                foreach (var selection in selectedCandidates)
                {
                    var block = new ContentBlock(
                        record.Context.SectionId,
                        selection.Title,
                        ContentBlockType.Question,
                        summary: null,
                        record.Context.DefaultDifficulty,
                        QuestionType.Unset,
                        ContentBlockStatus.Draft);
                    await _unitOfWork.ContentBlocks.AddAsync(block, transactionCancellationToken);
                    await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

                    var versionNumber = 1;
                    var docxPath = _pathProvider.GetContentBlockDocxPath(command.BankRootDirectory, block.Id, versionNumber);
                    var htmlPreviewPath = _pathProvider.GetContentBlockHtmlPreviewPath(command.BankRootDirectory, block.Id, versionNumber);
                    var plainTextPath = _pathProvider.GetContentBlockPlainTextPath(command.BankRootDirectory, block.Id, versionNumber);

                    generatedFilePaths.Add(docxPath);
                    await _questionImportProcessor.CreateNeutralizedCandidateDocxAsync(
                        selection.Candidate.DocxPath,
                        docxPath,
                        transactionCancellationToken);

                    generatedFilePaths.Add(htmlPreviewPath);
                    await _documentProcessor.GenerateHtmlPreviewAsync(docxPath, htmlPreviewPath, transactionCancellationToken);
                    var plainText = await _documentProcessor.ExtractPlainTextAsync(docxPath, transactionCancellationToken);
                    generatedFilePaths.Add(plainTextPath);
                    await _fileStore.SavePlainTextAsync(plainTextPath, plainText, transactionCancellationToken);

                    var version = new ContentBlockVersion(
                        block.Id,
                        versionNumber,
                        docxPath,
                        htmlPreviewPath,
                        plainText,
                        isCurrent: true);
                    await _unitOfWork.ContentBlockVersions.AddAsync(version, transactionCancellationToken);
                    await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

                    var parseResult = await _documentProcessor.GenerateQuestionPartHtmlPreviewAsync(
                        docxPath,
                        htmlPreviewPath,
                        transactionCancellationToken);
                    foreach (var part in parseResult.Parts)
                    {
                        await _unitOfWork.ContentBlockVersionParts.AddAsync(
                            new ContentBlockVersionPart(
                                version.Id,
                                part.PartType,
                                part.SortOrder,
                                part.PlainText,
                                JsonSerializer.Serialize(part.SourceStyleNames),
                                part.WarningMessage),
                            transactionCancellationToken);
                    }

                    version.MarkPartParsed(parseResult.Status, parseResult.Message);
                    version.MarkCurrent();
                    block.SetCurrentVersion(version.Id);
                    _unitOfWork.ContentBlockVersions.Update(version);
                    _unitOfWork.ContentBlocks.Update(block);

                    if (record.Context.AtomicSectionId.HasValue)
                    {
                        var item = new AtomicSectionItem(
                            record.Context.AtomicSectionId.Value,
                            block.Id,
                            ReferenceMode.FollowLatest,
                            lockedContentBlockVersionId: null,
                            sortOrder: 0,
                            atomicSectionPanelId: record.Context.AtomicSectionPanelId,
                            teachingRole: record.Context.DefaultTeachingRole);
                        await _unitOfWork.AtomicSectionItems.AddAsync(item, transactionCancellationToken);
                        atomicSectionItems.Add(item);
                    }
                    else
                    {
                        var item = new SectionItem(
                            record.Context.SectionId,
                            SectionItemTargetType.ContentBlock,
                            block.Id,
                            ReferenceMode.FollowLatest,
                            lockedContentBlockVersionId: null,
                            sortOrder: 0,
                            status: SectionStatus.Active);
                        await _unitOfWork.SectionItems.AddAsync(item, transactionCancellationToken);
                        sectionItems.Add(item);
                    }

                    contentBlockIds.Add(block.Id);
                    contentBlockVersionIds.Add(version.Id);
                }

                await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

                if (sectionItems.Count > 0)
                {
                    await InsertSectionItemsAsync(
                        record.Context.SectionId,
                        record.Context.AfterSectionItemId,
                        sectionItems,
                        transactionCancellationToken);
                }

                if (atomicSectionItems.Count > 0)
                {
                    await InsertAtomicSectionItemsAsync(
                        record.Context.AtomicSectionId!.Value,
                        record.Context.AtomicSectionPanelId,
                        record.Context.AfterAtomicSectionItemId,
                        atomicSectionItems,
                        transactionCancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

                return new QuestionImportConfirmResult(
                    contentBlockIds,
                    contentBlockVersionIds,
                    sectionItems.Select(item => item.Id).ToArray(),
                    atomicSectionItems.Select(item => item.Id).ToArray(),
                    sectionItems.Count > 0 ? "SectionItem" : atomicSectionItems.Count > 0 ? "AtomicSectionItem" : null,
                    sectionItems.Count > 0 ? sectionItems[0].Id : atomicSectionItems.Count > 0 ? atomicSectionItems[0].Id : null);
            }, cancellationToken);

            record = record with
            {
                Status = QuestionImportSessionStatus.Imported,
                UpdatedTime = DateTimeOffset.UtcNow
            };
            await SaveSessionRecordAsync(command.BankRootDirectory, record, CancellationToken.None);
            CleanupSessionDirectory(command.BankRootDirectory, record.SessionId);

            return result;
        }
        catch (Exception exception)
        {
            await CleanupGeneratedFilesAsync(generatedFilePaths);
            var failedRecord = record with
            {
                Status = QuestionImportSessionStatus.Failed,
                Message = exception.Message,
                UpdatedTime = DateTimeOffset.UtcNow
            };
            await SaveSessionRecordAsync(command.BankRootDirectory, failedRecord, CancellationToken.None);
            throw;
        }
    }

    public async Task<QuestionImportSessionDto> CancelSessionAsync(
        CancelQuestionImportSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(command.BankRootDirectory);
        var record = await LoadSessionRecordAsync(command.BankRootDirectory, command.SessionId, cancellationToken);
        var cancelled = record with
        {
            Status = QuestionImportSessionStatus.Cancelled,
            UpdatedTime = DateTimeOffset.UtcNow
        };

        await SaveSessionRecordAsync(command.BankRootDirectory, cancelled, cancellationToken);
        CleanupSessionDirectory(command.BankRootDirectory, command.SessionId);

        return await ToDtoAsync(cancelled, cancellationToken);
    }

    private async Task<QuestionImportSessionRecord> ParseSessionAsync(
        string bankRootDirectory,
        QuestionImportSessionRecord record,
        CancellationToken cancellationToken)
    {
        record = record with
        {
            Status = QuestionImportSessionStatus.Parsing,
            Message = null,
            UpdatedTime = DateTimeOffset.UtcNow
        };
        await SaveSessionRecordAsync(bankRootDirectory, record, cancellationToken);

        try
        {
            var candidatesDirectory = Path.Combine(GetSessionDirectory(bankRootDirectory, record.SessionId), "candidates");
            if (Directory.Exists(candidatesDirectory))
            {
                Directory.Delete(candidatesDirectory, recursive: true);
            }

            var candidateDocuments = await _questionImportProcessor.SplitCandidatesAsync(
                record.SourceDocxPath,
                candidatesDirectory,
                cancellationToken);
            var candidates = new List<QuestionImportCandidateRecord>();

            foreach (var candidateDocument in candidateDocuments)
            {
                QuestionPartParseResult? parseResult = null;
                string? parseMessage = null;
                try
                {
                    parseResult = await _documentProcessor.GenerateQuestionPartHtmlPreviewAsync(
                        candidateDocument.DocxPath,
                        candidateDocument.HtmlPreviewPath,
                        cancellationToken);
                }
                catch (Exception exception)
                {
                    parseMessage = exception.Message;
                }

                candidates.Add(new QuestionImportCandidateRecord(
                    candidateDocument.CandidateId,
                    candidateDocument.SortOrder,
                    candidateDocument.DocxPath,
                    candidateDocument.HtmlPreviewPath,
                    parseResult?.Status ?? ContentBlockPartParseStatus.Failed,
                    parseResult?.Message ?? parseMessage,
                    parseResult?.Parts.Select(part => new QuestionImportCandidatePartDto(
                        part.PartType,
                        part.SortOrder,
                        part.PlainText ?? string.Empty,
                        part.SourceStyleNames,
                        part.WarningMessage)).ToArray() ?? []));
            }

            record = record with
            {
                Status = QuestionImportSessionStatus.ReadyForReview,
                Message = null,
                UpdatedTime = DateTimeOffset.UtcNow,
                Candidates = candidates
            };
            await SaveSessionRecordAsync(bankRootDirectory, record, cancellationToken);
            return record;
        }
        catch (Exception exception)
        {
            record = record with
            {
                Status = QuestionImportSessionStatus.Failed,
                Message = exception.Message,
                UpdatedTime = DateTimeOffset.UtcNow
            };
            await SaveSessionRecordAsync(bankRootDirectory, record, cancellationToken);
            return record;
        }
    }

    private async Task InsertSectionItemsAsync(
        int sectionId,
        int? afterSectionItemId,
        IReadOnlyList<SectionItem> insertedItems,
        CancellationToken cancellationToken)
    {
        var existingItems = (await _unitOfWork.SectionItems.ListBySectionAsync(sectionId, cancellationToken))
            .Where(item => item.ParentItemId is null && insertedItems.All(inserted => inserted.Id != item.Id))
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToList();
        var insertIndex = existingItems.Count;

        if (afterSectionItemId.HasValue)
        {
            insertIndex = existingItems.FindIndex(item => item.Id == afterSectionItemId.Value);
            if (insertIndex < 0)
            {
                throw new CmsV2ApplicationException($"SectionItem {afterSectionItemId.Value} was not found in Section {sectionId}.");
            }

            insertIndex += 1;
        }

        existingItems.InsertRange(insertIndex, insertedItems);
        for (var index = 0; index < existingItems.Count; index++)
        {
            existingItems[index].ChangeSortOrder((index + 1) * 10);
            _unitOfWork.SectionItems.Update(existingItems[index]);
        }
    }

    private async Task InsertAtomicSectionItemsAsync(
        int atomicSectionId,
        int? atomicSectionPanelId,
        int? afterAtomicSectionItemId,
        IReadOnlyList<AtomicSectionItem> insertedItems,
        CancellationToken cancellationToken)
    {
        var existingItems = (await _unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSectionId, cancellationToken))
            .Where(item => item.AtomicSectionPanelId == atomicSectionPanelId && insertedItems.All(inserted => inserted.Id != item.Id))
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToList();
        var insertIndex = existingItems.Count;

        if (afterAtomicSectionItemId.HasValue)
        {
            insertIndex = existingItems.FindIndex(item => item.Id == afterAtomicSectionItemId.Value);
            if (insertIndex < 0)
            {
                throw new CmsV2ApplicationException($"AtomicSectionItem {afterAtomicSectionItemId.Value} was not found in AtomicSection {atomicSectionId}.");
            }

            insertIndex += 1;
        }

        existingItems.InsertRange(insertIndex, insertedItems);
        for (var index = 0; index < existingItems.Count; index++)
        {
            existingItems[index].ChangeSortOrder((index + 1) * 10);
            _unitOfWork.AtomicSectionItems.Update(existingItems[index]);
        }
    }

    private static IReadOnlyList<SelectedQuestionImportCandidate> ResolveSelectedCandidates(
        QuestionImportSessionRecord record,
        IReadOnlyList<ConfirmQuestionImportCandidateSelection> selections)
    {
        var candidatesById = record.Candidates.ToDictionary(candidate => candidate.CandidateId, StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var selected = new List<SelectedQuestionImportCandidate>();

        foreach (var selection in selections)
        {
            if (!seen.Add(selection.CandidateId))
            {
                throw new CmsV2ApplicationException("Question import candidate ids must be distinct.");
            }

            if (!candidatesById.TryGetValue(selection.CandidateId, out var candidate))
            {
                throw new CmsV2ApplicationException($"Question import candidate {selection.CandidateId} was not found.");
            }

            if (selection.Selected)
            {
                selected.Add(new SelectedQuestionImportCandidate(candidate, selection.Title ?? string.Empty));
            }
        }

        return selected.OrderBy(item => item.Candidate.SortOrder).ToArray();
    }

    private async Task<InsertQuestionContext> ValidateContextAsync(InsertQuestionContext context, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Sections.GetByIdAsync(context.SectionId, cancellationToken) is null)
        {
            throw new CmsV2ApplicationException($"Section {context.SectionId} was not found.");
        }

        if (context.AtomicSectionId.HasValue)
        {
            if (context.AfterSectionItemId.HasValue)
            {
                throw new CmsV2ApplicationException("afterSectionItemId cannot be used with AtomicSection import context.");
            }

            var atomicSection = await _unitOfWork.AtomicSections.GetByIdAsync(context.AtomicSectionId.Value, cancellationToken)
                ?? throw new CmsV2ApplicationException($"AtomicSection {context.AtomicSectionId.Value} was not found.");
            if (atomicSection.SectionId != context.SectionId)
            {
                throw new CmsV2ApplicationException("AtomicSection import context must belong to the requested Section.");
            }

            if (context.AtomicSectionPanelId.HasValue)
            {
                var panel = await _unitOfWork.AtomicSectionPanels.GetByIdAsync(
                    context.AtomicSectionPanelId.Value,
                    cancellationToken)
                    ?? throw new CmsV2ApplicationException($"AtomicSectionPanel {context.AtomicSectionPanelId.Value} was not found.");
                if (panel.AtomicSectionId != context.AtomicSectionId.Value)
                {
                    throw new CmsV2ApplicationException("AtomicSectionPanel import context must belong to the requested AtomicSection.");
                }

                context = context with
                {
                    DefaultTeachingRole = panel.TeachingRole,
                    DefaultDifficulty = panel.Difficulty
                };
            }

            if (context.AfterAtomicSectionItemId.HasValue)
            {
                var afterItem = await _unitOfWork.AtomicSectionItems.GetByIdAsync(context.AfterAtomicSectionItemId.Value, cancellationToken)
                    ?? throw new CmsV2ApplicationException($"AtomicSectionItem {context.AfterAtomicSectionItemId.Value} was not found.");
                if (afterItem.AtomicSectionId != context.AtomicSectionId.Value)
                {
                    throw new CmsV2ApplicationException("afterAtomicSectionItemId must belong to the requested AtomicSection.");
                }

                if (context.AtomicSectionPanelId.HasValue
                    && afterItem.AtomicSectionPanelId != context.AtomicSectionPanelId.Value)
                {
                    throw new CmsV2ApplicationException("afterAtomicSectionItemId must belong to the same AtomicSectionPanel.");
                }
            }
        }
        else
        {
            if (context.AtomicSectionPanelId.HasValue)
            {
                throw new CmsV2ApplicationException("AtomicSectionPanel import context requires AtomicSection import context.");
            }

            if (context.AfterAtomicSectionItemId.HasValue)
            {
                throw new CmsV2ApplicationException("afterAtomicSectionItemId requires AtomicSection import context.");
            }

            if (context.AfterSectionItemId.HasValue)
            {
                var afterItem = await _unitOfWork.SectionItems.GetByIdAsync(context.AfterSectionItemId.Value, cancellationToken)
                    ?? throw new CmsV2ApplicationException($"SectionItem {context.AfterSectionItemId.Value} was not found.");
                if (afterItem.SectionId != context.SectionId)
                {
                    throw new CmsV2ApplicationException("afterSectionItemId must belong to the requested Section.");
                }
            }
        }

        return context;
    }

    private static void EnsureSessionCanReopen(QuestionImportSessionRecord record)
    {
        if (record.Status is QuestionImportSessionStatus.Cancelled
            or QuestionImportSessionStatus.Expired
            or QuestionImportSessionStatus.Imported
            or QuestionImportSessionStatus.Importing)
        {
            throw new CmsV2ApplicationException($"Question import session cannot be reopened when status is {record.Status}.");
        }
    }

    private async Task<QuestionImportSessionDto> ToDtoAsync(
        QuestionImportSessionRecord record,
        CancellationToken cancellationToken)
    {
        var candidates = new List<QuestionImportCandidateDto>();
        foreach (var candidate in record.Candidates.OrderBy(candidate => candidate.SortOrder))
        {
            var htmlPreview = await ReadOptionalTextAsync(candidate.HtmlPreviewPath, cancellationToken);
            candidates.Add(new QuestionImportCandidateDto(
                candidate.CandidateId,
                candidate.SortOrder,
                candidate.ParseStatus,
                candidate.ParseMessage,
                htmlPreview,
                candidate.Parts));
        }

        return new QuestionImportSessionDto(
            record.SessionId,
            record.Context,
            record.Status,
            record.Message,
            record.CreatedTime,
            record.UpdatedTime,
            candidates);
    }

    private static async Task<string?> ReadOptionalTextAsync(string filePath, CancellationToken cancellationToken)
    {
        return File.Exists(filePath)
            ? await File.ReadAllTextAsync(filePath, cancellationToken)
            : null;
    }

    private static async Task SaveSessionRecordAsync(
        string bankRootDirectory,
        QuestionImportSessionRecord record,
        CancellationToken cancellationToken)
    {
        var sessionDirectory = GetSessionDirectory(bankRootDirectory, record.SessionId);
        Directory.CreateDirectory(sessionDirectory);
        await File.WriteAllTextAsync(
            GetSessionRecordPath(bankRootDirectory, record.SessionId),
            JsonSerializer.Serialize(record, JsonOptions),
            cancellationToken);
    }

    private static async Task<QuestionImportSessionRecord> LoadSessionRecordAsync(
        string bankRootDirectory,
        string sessionId,
        CancellationToken cancellationToken)
    {
        var recordPath = GetSessionRecordPath(bankRootDirectory, sessionId);
        if (!File.Exists(recordPath))
        {
            throw new CmsV2ApplicationException($"Question import session {sessionId} was not found.");
        }

        var json = await File.ReadAllTextAsync(recordPath, cancellationToken);
        return JsonSerializer.Deserialize<QuestionImportSessionRecord>(json, JsonOptions)
            ?? throw new CmsV2ApplicationException($"Question import session {sessionId} is invalid.");
    }

    private async Task CleanupGeneratedFilesAsync(IReadOnlyCollection<string> generatedFilePaths)
    {
        foreach (var filePath in generatedFilePaths.Distinct(StringComparer.OrdinalIgnoreCase).Reverse())
        {
            try
            {
                await _fileStore.DeleteIfExistsAsync(filePath, CancellationToken.None);
            }
            catch
            {
            }
        }
    }

    private static void CleanupSessionDirectory(string bankRootDirectory, string sessionId)
    {
        var sessionDirectory = GetSessionDirectory(bankRootDirectory, sessionId);
        if (Directory.Exists(sessionDirectory))
        {
            Directory.Delete(sessionDirectory, recursive: true);
        }
    }

    private static string GetSessionDirectory(string bankRootDirectory, string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new CmsV2ApplicationException("Question import session id cannot be empty.");
        }

        return Path.Combine(
            Path.GetFullPath(bankRootDirectory.Trim()),
            "edit-sessions",
            "question-imports",
            sessionId.Trim());
    }

    private static string GetSourceDocxPath(string bankRootDirectory, string sessionId)
    {
        return Path.Combine(GetSessionDirectory(bankRootDirectory, sessionId), "source.docx");
    }

    private static string GetSessionRecordPath(string bankRootDirectory, string sessionId)
    {
        return Path.Combine(GetSessionDirectory(bankRootDirectory, sessionId), "session.json");
    }

    private static void ValidateBankRootDirectory(string bankRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(bankRootDirectory))
        {
            throw new CmsV2ApplicationException("BankRootDirectory cannot be empty.");
        }
    }

    private sealed record SelectedQuestionImportCandidate(
        QuestionImportCandidateRecord Candidate,
        string Title);
}

public sealed record InsertQuestionContext(
    int SectionId,
    int? AtomicSectionId,
    int? AtomicSectionPanelId,
    int? AfterAtomicSectionItemId,
    int? AfterSectionItemId,
    AtomicSectionTeachingRole DefaultTeachingRole,
    Difficulty DefaultDifficulty);

public sealed record CreateQuestionImportSessionCommand(
    string BankRootDirectory,
    InsertQuestionContext Context,
    bool OpenWord);

public sealed record GetQuestionImportSessionCommand(
    string BankRootDirectory,
    string SessionId);

public sealed record ReopenQuestionImportSessionCommand(
    string BankRootDirectory,
    string SessionId);

public sealed record ConfirmQuestionImportCommand(
    string BankRootDirectory,
    string SessionId,
    IReadOnlyList<ConfirmQuestionImportCandidateSelection> Candidates);

public sealed record ConfirmQuestionImportCandidateSelection(
    string CandidateId,
    bool Selected,
    string Title);

public sealed record CancelQuestionImportSessionCommand(
    string BankRootDirectory,
    string SessionId);

public sealed record QuestionImportSessionDto(
    string SessionId,
    InsertQuestionContext Context,
    QuestionImportSessionStatus Status,
    string? Message,
    DateTimeOffset CreatedTime,
    DateTimeOffset UpdatedTime,
    IReadOnlyList<QuestionImportCandidateDto> Candidates);

public sealed record QuestionImportCandidateDto(
    string CandidateId,
    int SortOrder,
    ContentBlockPartParseStatus ParseStatus,
    string? ParseMessage,
    string? HtmlPreview,
    IReadOnlyList<QuestionImportCandidatePartDto> Parts);

public sealed record QuestionImportCandidatePartDto(
    ContentBlockPartType PartType,
    int SortOrder,
    string PlainText,
    IReadOnlyList<string> SourceStyleNames,
    string? WarningMessage);

public sealed record QuestionImportConfirmResult(
    IReadOnlyList<int> ContentBlockIds,
    IReadOnlyList<int> ContentBlockVersionIds,
    IReadOnlyList<int> SectionItemIds,
    IReadOnlyList<int> AtomicSectionItemIds,
    string? FirstInsertedNodeType,
    int? FirstInsertedNodeId);

internal sealed record QuestionImportSessionRecord(
    string SessionId,
    InsertQuestionContext Context,
    QuestionImportSessionStatus Status,
    string? Message,
    DateTimeOffset CreatedTime,
    DateTimeOffset UpdatedTime,
    string SourceDocxPath,
    IReadOnlyList<QuestionImportCandidateRecord> Candidates);

internal sealed record QuestionImportCandidateRecord(
    string CandidateId,
    int SortOrder,
    string DocxPath,
    string HtmlPreviewPath,
    ContentBlockPartParseStatus ParseStatus,
    string? ParseMessage,
    IReadOnlyList<QuestionImportCandidatePartDto> Parts);
