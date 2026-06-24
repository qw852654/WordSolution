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

    public QuestionImportUseCases(
        ICmsV2UnitOfWork unitOfWork,
        ICmsV2FileAssetPathProvider pathProvider,
        IContentBlockFileStore fileStore,
        IContentBlockDocumentProcessor documentProcessor,
        IQuestionImportDocumentProcessor questionImportProcessor)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
        _documentProcessor = documentProcessor ?? throw new ArgumentNullException(nameof(documentProcessor));
        _questionImportProcessor = questionImportProcessor ?? throw new ArgumentNullException(nameof(questionImportProcessor));
    }

    public async Task<QuestionImportSessionDto> CreateSessionAsync(
        CreateQuestionImportSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(command.BankRootDirectory);
        ValidateDocxStream(command.DocxStream);
        await RequireSectionAsync(command.SectionId, cancellationToken);

        var sessionId = Guid.NewGuid().ToString("N");
        var sessionDirectory = GetSessionDirectory(command.BankRootDirectory, sessionId);
        var originalDocxPath = Path.Combine(sessionDirectory, "original.docx");
        Directory.CreateDirectory(sessionDirectory);
        await _fileStore.SaveContentBlockDocxAsync(originalDocxPath, command.DocxStream, cancellationToken);

        var candidatesDirectory = Path.Combine(sessionDirectory, "candidates");
        var candidateDocuments = await _questionImportProcessor.SplitCandidatesAsync(
            originalDocxPath,
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

        var record = new QuestionImportSessionRecord(
            sessionId,
            command.SectionId,
            DateTimeOffset.UtcNow,
            originalDocxPath,
            candidates);
        await SaveSessionRecordAsync(command.BankRootDirectory, record, cancellationToken);

        return await ToDtoAsync(record, cancellationToken);
    }

    public async Task<QuestionImportSessionDto> GetSessionAsync(
        GetQuestionImportSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(command.BankRootDirectory);
        var record = await LoadSessionRecordAsync(command.BankRootDirectory, command.SessionId, cancellationToken);
        return await ToDtoAsync(record, cancellationToken);
    }

    public async Task<ContentBlockDocumentVersionResult> ConfirmCandidateAsync(
        ConfirmQuestionImportCandidateCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(command.BankRootDirectory);
        var record = await LoadSessionRecordAsync(command.BankRootDirectory, command.SessionId, cancellationToken);
        if (record.SectionId != command.SectionId)
        {
            throw new CmsV2ApplicationException("Question import session does not belong to the requested Section.");
        }

        var candidate = record.Candidates.SingleOrDefault(item => item.CandidateId == command.CandidateId)
            ?? throw new CmsV2ApplicationException($"Question import candidate {command.CandidateId} was not found.");
        var generatedFilePaths = new List<string>();

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
            {
                await RequireSectionAsync(command.SectionId, transactionCancellationToken);
                var block = new ContentBlock(
                    command.SectionId,
                    command.Title,
                    command.BlockType,
                    command.Summary,
                    command.Difficulty,
                    command.QuestionType,
                    ContentBlockStatus.Draft);
                await _unitOfWork.ContentBlocks.AddAsync(block, transactionCancellationToken);
                await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

                var versionNumber = 1;
                var docxPath = _pathProvider.GetContentBlockDocxPath(command.BankRootDirectory, block.Id, versionNumber);
                var htmlPreviewPath = _pathProvider.GetContentBlockHtmlPreviewPath(command.BankRootDirectory, block.Id, versionNumber);
                var plainTextPath = _pathProvider.GetContentBlockPlainTextPath(command.BankRootDirectory, block.Id, versionNumber);

                await _questionImportProcessor.CreateNeutralizedCandidateDocxAsync(
                    candidate.DocxPath,
                    docxPath,
                    transactionCancellationToken);
                generatedFilePaths.Add(docxPath);
                await _documentProcessor.GenerateHtmlPreviewAsync(docxPath, htmlPreviewPath, transactionCancellationToken);
                generatedFilePaths.Add(htmlPreviewPath);
                var plainText = await _documentProcessor.ExtractPlainTextAsync(docxPath, transactionCancellationToken);
                await _fileStore.SavePlainTextAsync(plainTextPath, plainText, transactionCancellationToken);
                generatedFilePaths.Add(plainTextPath);

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
                await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

                return new ContentBlockDocumentVersionResult(
                    block.Id,
                    version.Id,
                    version.VersionNumber,
                    docxPath,
                    htmlPreviewPath,
                    plainTextPath);
            }, cancellationToken);
        }
        catch
        {
            await CleanupGeneratedFilesAsync(generatedFilePaths);
            throw;
        }
    }

    public Task CancelSessionAsync(CancelQuestionImportSessionCommand command, CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(command.BankRootDirectory);
        var sessionDirectory = GetSessionDirectory(command.BankRootDirectory, command.SessionId);
        if (Directory.Exists(sessionDirectory))
        {
            Directory.Delete(sessionDirectory, recursive: true);
        }

        return Task.CompletedTask;
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
            record.SectionId,
            record.CreatedTime,
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

    private async Task RequireSectionAsync(int sectionId, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Sections.GetByIdAsync(sectionId, cancellationToken) is null)
        {
            throw new CmsV2ApplicationException($"Section {sectionId} was not found.");
        }
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

    private static string GetSessionDirectory(string bankRootDirectory, string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new CmsV2ApplicationException("Question import session id cannot be empty.");
        }

        return Path.Combine(
            Path.GetFullPath(bankRootDirectory.Trim()),
            "import-sessions",
            "questions",
            sessionId.Trim());
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

    private static void ValidateDocxStream(Stream docxStream)
    {
        if (docxStream is null || !docxStream.CanRead || docxStream.CanSeek && docxStream.Length == 0)
        {
            throw new CmsV2ApplicationException("Question import DOCX stream must be readable and not empty.");
        }
    }
}

public sealed record CreateQuestionImportSessionCommand(
    string BankRootDirectory,
    int SectionId,
    Stream DocxStream);

public sealed record GetQuestionImportSessionCommand(
    string BankRootDirectory,
    string SessionId);

public sealed record ConfirmQuestionImportCandidateCommand(
    string BankRootDirectory,
    string SessionId,
    string CandidateId,
    int SectionId,
    string Title,
    string? Summary,
    ContentBlockType BlockType,
    Difficulty Difficulty,
    QuestionType? QuestionType);

public sealed record CancelQuestionImportSessionCommand(
    string BankRootDirectory,
    string SessionId);

public sealed record QuestionImportSessionDto(
    string SessionId,
    int SectionId,
    DateTimeOffset CreatedTime,
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

internal sealed record QuestionImportSessionRecord(
    string SessionId,
    int SectionId,
    DateTimeOffset CreatedTime,
    string OriginalDocxPath,
    IReadOnlyList<QuestionImportCandidateRecord> Candidates);

internal sealed record QuestionImportCandidateRecord(
    string CandidateId,
    int SortOrder,
    string DocxPath,
    string HtmlPreviewPath,
    ContentBlockPartParseStatus ParseStatus,
    string? ParseMessage,
    IReadOnlyList<QuestionImportCandidatePartDto> Parts);
