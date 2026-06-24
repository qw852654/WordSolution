using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Repositories;
using System.Text.Json;

namespace WordSolution.CmsV2.Application.ContentBlocks;

public sealed class ContentBlockDocumentUseCases
{
    private readonly ICmsV2UnitOfWork _unitOfWork;
    private readonly ICmsV2FileAssetPathProvider _pathProvider;
    private readonly IContentBlockFileStore _fileStore;
    private readonly IContentBlockDocumentProcessor _documentProcessor;

    public ContentBlockDocumentUseCases(
        ICmsV2UnitOfWork unitOfWork,
        ICmsV2FileAssetPathProvider pathProvider,
        IContentBlockFileStore fileStore,
        IContentBlockDocumentProcessor documentProcessor)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
        _documentProcessor = documentProcessor ?? throw new ArgumentNullException(nameof(documentProcessor));
    }

    public async Task<ContentBlockDocumentVersionResult> CreateContentBlockWithBlankDocumentAsync(
        CreateContentBlockWithBlankDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(command.BankRootDirectory);

        var generatedFilePaths = new List<string>();

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
            {
                await RequireSectionAsync(command.SectionId, transactionCancellationToken);

                var contentBlock = new ContentBlock(
                    command.SectionId,
                    command.Title,
                    command.BlockType,
                    command.Summary,
                    command.Difficulty,
                    command.QuestionType,
                    command.Status);

                await _unitOfWork.ContentBlocks.AddAsync(contentBlock, transactionCancellationToken);
                await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

                var result = await CreateVersionFilesAndMetadataAsync(
                    command.BankRootDirectory,
                    contentBlock,
                    versionNumber: 1,
                    docxStream: null,
                    setAsCurrent: true,
                    generatedFilePaths,
                    transactionCancellationToken);

                return result;
            }, cancellationToken);
        }
        catch (CmsV2ApplicationException)
        {
            await CleanupGeneratedFilesAsync(generatedFilePaths);
            throw;
        }
        catch (Exception exception)
        {
            await CleanupGeneratedFilesAsync(generatedFilePaths);
            throw new CmsV2ApplicationException("ContentBlock document processing failed.", exception);
        }
    }

    public async Task<ContentBlockDocumentVersionResult> CreateBlankContentBlockVersionAsync(
        CreateBlankContentBlockVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(command.BankRootDirectory);

        var generatedFilePaths = new List<string>();

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
            {
                var contentBlock = await RequireContentBlockAsync(command.ContentBlockId, transactionCancellationToken);
                var existingVersions = await _unitOfWork.ContentBlockVersions.ListByContentBlockAsync(
                    command.ContentBlockId,
                    transactionCancellationToken);
                var nextVersionNumber = existingVersions.Count == 0
                    ? 1
                    : existingVersions.Max(version => version.VersionNumber) + 1;

                var result = await CreateVersionFilesAndMetadataAsync(
                    command.BankRootDirectory,
                    contentBlock,
                    nextVersionNumber,
                    docxStream: null,
                    command.SetAsCurrent,
                    generatedFilePaths,
                    transactionCancellationToken);

                if (command.SetAsCurrent)
                {
                    foreach (var existingVersion in existingVersions)
                    {
                        existingVersion.MarkNotCurrent();
                        _unitOfWork.ContentBlockVersions.Update(existingVersion);
                    }

                    await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
                }

                return result;
            }, cancellationToken);
        }
        catch (CmsV2ApplicationException)
        {
            await CleanupGeneratedFilesAsync(generatedFilePaths);
            throw;
        }
        catch (Exception exception)
        {
            await CleanupGeneratedFilesAsync(generatedFilePaths);
            throw new CmsV2ApplicationException("ContentBlock document processing failed.", exception);
        }
    }

    public async Task<ContentBlockDocumentVersionResult> ImportContentBlockDocxVersionAsync(
        ImportContentBlockDocxVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(command.BankRootDirectory);
        ValidateDocxStream(command.DocxStream);

        var generatedFilePaths = new List<string>();

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
            {
                var contentBlock = await RequireContentBlockAsync(command.ContentBlockId, transactionCancellationToken);
                var existingVersions = await _unitOfWork.ContentBlockVersions.ListByContentBlockAsync(
                    command.ContentBlockId,
                    transactionCancellationToken);
                var nextVersionNumber = existingVersions.Count == 0
                    ? 1
                    : existingVersions.Max(version => version.VersionNumber) + 1;

                var result = await CreateVersionFilesAndMetadataAsync(
                    command.BankRootDirectory,
                    contentBlock,
                    nextVersionNumber,
                    command.DocxStream,
                    command.SetAsCurrent,
                    generatedFilePaths,
                    transactionCancellationToken);

                if (command.SetAsCurrent)
                {
                    foreach (var existingVersion in existingVersions)
                    {
                        existingVersion.MarkNotCurrent();
                        _unitOfWork.ContentBlockVersions.Update(existingVersion);
                    }

                    await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
                }

                return result;
            }, cancellationToken);
        }
        catch (CmsV2ApplicationException)
        {
            await CleanupGeneratedFilesAsync(generatedFilePaths);
            throw;
        }
        catch (Exception exception)
        {
            await CleanupGeneratedFilesAsync(generatedFilePaths);
            throw new CmsV2ApplicationException("ContentBlock document processing failed.", exception);
        }
    }

    private async Task<ContentBlockDocumentVersionResult> CreateVersionFilesAndMetadataAsync(
        string bankRootDirectory,
        ContentBlock contentBlock,
        int versionNumber,
        Stream? docxStream,
        bool setAsCurrent,
        List<string> generatedFilePaths,
        CancellationToken cancellationToken)
    {
        var docxPath = _pathProvider.GetContentBlockDocxPath(bankRootDirectory, contentBlock.Id, versionNumber);
        var htmlPreviewPath = _pathProvider.GetContentBlockHtmlPreviewPath(bankRootDirectory, contentBlock.Id, versionNumber);
        var plainTextPath = _pathProvider.GetContentBlockPlainTextPath(bankRootDirectory, contentBlock.Id, versionNumber);

        if (docxStream is null)
        {
            await _documentProcessor.CreateBlankDocxAsync(docxPath, cancellationToken);
        }
        else
        {
            await _fileStore.SaveContentBlockDocxAsync(docxPath, docxStream, cancellationToken);
        }

        generatedFilePaths.Add(docxPath);

        await _documentProcessor.GenerateHtmlPreviewAsync(docxPath, htmlPreviewPath, cancellationToken);
        generatedFilePaths.Add(htmlPreviewPath);

        var plainText = await _documentProcessor.ExtractPlainTextAsync(docxPath, cancellationToken);
        await _fileStore.SavePlainTextAsync(plainTextPath, plainText, cancellationToken);
        generatedFilePaths.Add(plainTextPath);

        var version = new ContentBlockVersion(
            contentBlock.Id,
            versionNumber,
            docxPath,
            htmlPreviewPath,
            plainText,
            isCurrent: setAsCurrent);

        await _unitOfWork.ContentBlockVersions.AddAsync(version, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (ShouldGenerateQuestionParts(contentBlock.BlockType))
        {
            await GenerateQuestionPartsAsync(version, docxPath, htmlPreviewPath, cancellationToken);
        }

        if (setAsCurrent)
        {
            version.MarkCurrent();
            contentBlock.SetCurrentVersion(version.Id);
            _unitOfWork.ContentBlockVersions.Update(version);
            _unitOfWork.ContentBlocks.Update(contentBlock);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new ContentBlockDocumentVersionResult(
            contentBlock.Id,
            version.Id,
            version.VersionNumber,
            docxPath,
            htmlPreviewPath,
            plainTextPath);
    }

    private async Task GenerateQuestionPartsAsync(
        ContentBlockVersion version,
        string docxPath,
        string htmlPreviewPath,
        CancellationToken cancellationToken)
    {
        try
        {
            var parseResult = await _documentProcessor.GenerateQuestionPartHtmlPreviewAsync(
                docxPath,
                htmlPreviewPath,
                cancellationToken);

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
                    cancellationToken);
            }

            version.MarkPartParsed(parseResult.Status, parseResult.Message);
            _unitOfWork.ContentBlockVersions.Update(version);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            version.MarkPartParsed(ContentBlockPartParseStatus.Failed, exception.Message);
            _unitOfWork.ContentBlockVersions.Update(version);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private static bool ShouldGenerateQuestionParts(ContentBlockType blockType)
    {
        return blockType is ContentBlockType.Question;
    }

    private async Task<ContentBlock> RequireContentBlockAsync(int contentBlockId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ContentBlocks.GetByIdAsync(contentBlockId, cancellationToken)
            ?? throw new CmsV2ApplicationException($"ContentBlock {contentBlockId} was not found.");
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
                // Best-effort cleanup only; the original application failure is more important.
            }
        }
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
        if (docxStream is null)
        {
            throw new CmsV2ApplicationException("DocxStream cannot be null.");
        }

        if (!docxStream.CanRead)
        {
            throw new CmsV2ApplicationException("DocxStream must be readable.");
        }

        if (docxStream.CanSeek && docxStream.Length == 0)
        {
            throw new CmsV2ApplicationException("DocxStream cannot be empty.");
        }
    }
}
