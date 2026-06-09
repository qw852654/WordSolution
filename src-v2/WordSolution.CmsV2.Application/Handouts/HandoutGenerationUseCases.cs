using System.Globalization;
using System.Text.Json;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Application.Handouts;

public sealed class HandoutGenerationUseCases
{
    private const int MaxContentBlockExpandDepth = 10;

    private readonly ICmsV2UnitOfWork _unitOfWork;
    private readonly ICmsV2FileAssetPathProvider _pathProvider;
    private readonly IContentBlockFileStore _fileStore;
    private readonly IHandoutDocumentGenerator _documentGenerator;

    public HandoutGenerationUseCases(
        ICmsV2UnitOfWork unitOfWork,
        ICmsV2FileAssetPathProvider pathProvider,
        IContentBlockFileStore fileStore,
        IHandoutDocumentGenerator documentGenerator)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
        _documentGenerator = documentGenerator ?? throw new ArgumentNullException(nameof(documentGenerator));
    }

    public async Task<GeneratedHandoutFileResult> GenerateHandoutWordAsync(
        GenerateHandoutWordCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.BankRootDirectory))
        {
            throw new CmsV2ApplicationException("BankRootDirectory cannot be empty.");
        }

        var generatedTime = command.GeneratedTime ?? DateTimeOffset.UtcNow;
        string? outputDocxPath = null;

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
            {
                var outputForm = await RequireOutputFormAsync(command.OutputFormId, transactionCancellationToken);
                if (outputForm.OutputFormat != OutputFormat.Word)
                {
                    throw new CmsV2ApplicationException("Only Word output is supported in the current stage.");
                }

                var handoutVersion = await RequireHandoutVersionAsync(
                    outputForm.HandoutVersionId,
                    transactionCancellationToken);
                var outputTemplate = await RequireOutputTemplateAsync(
                    outputForm.OutputTemplateId,
                    transactionCancellationToken);

                if (!await _fileStore.ExistsAsync(outputTemplate.TemplateDocxPath, transactionCancellationToken))
                {
                    throw new CmsV2ApplicationException($"OutputTemplate file was not found: {outputTemplate.TemplateDocxPath}");
                }

                var resolvedSources = await ResolveHandoutSourcesAsync(
                    handoutVersion.Id,
                    transactionCancellationToken);
                outputDocxPath = _pathProvider.GetGeneratedHandoutDocxPath(
                    command.BankRootDirectory,
                    handoutVersion.Id,
                    outputForm.Id,
                    outputForm.Title,
                    generatedTime);

                var documentSources = resolvedSources
                    .Select(source => new HandoutDocumentSource(source.Title, source.DocxPath))
                    .ToArray();

                await _documentGenerator.GenerateWordAsync(
                    handoutVersion.Title,
                    outputTemplate.TemplateDocxPath,
                    documentSources,
                    outputDocxPath,
                    generatedTime,
                    transactionCancellationToken);

                var manifestJson = CreateVersionManifestJson(
                    outputForm.Id,
                    handoutVersion.Id,
                    generatedTime,
                    resolvedSources);
                var generatedFile = new GeneratedFile(
                    outputForm.Id,
                    outputDocxPath,
                    manifestJson,
                    generatedTime);

                await _unitOfWork.GeneratedFiles.AddAsync(generatedFile, transactionCancellationToken);
                await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

                return new GeneratedHandoutFileResult(
                    generatedFile.Id,
                    outputForm.Id,
                    handoutVersion.Id,
                    outputDocxPath,
                    manifestJson);
            }, cancellationToken);
        }
        catch (CmsV2ApplicationException)
        {
            await CleanupOutputFileAsync(outputDocxPath);
            throw;
        }
        catch (Exception exception)
        {
            await CleanupOutputFileAsync(outputDocxPath);
            throw new CmsV2ApplicationException("Handout generation failed.", exception);
        }
    }

    private async Task<IReadOnlyList<ResolvedHandoutSource>> ResolveHandoutSourcesAsync(
        int handoutVersionId,
        CancellationToken cancellationToken)
    {
        var handoutItems = await _unitOfWork.HandoutVersionItems.ListByHandoutVersionAsync(
            handoutVersionId,
            cancellationToken);
        var sources = new List<ResolvedHandoutSource>();

        foreach (var item in handoutItems)
        {
            if (item.TargetType == HandoutVersionItemTargetType.ContentBlock)
            {
                await ResolveContentBlockTreeAsync(
                    item.TargetId,
                    lockedContentBlockVersionId: null,
                    item.TitleOverride,
                    sources,
                    new HashSet<int>(),
                    depth: 1,
                    cancellationToken);
            }
            else if (item.TargetType == HandoutVersionItemTargetType.SectionVariant)
            {
                await ResolveSectionVariantAsync(item.TargetId, sources, cancellationToken);
            }
            else
            {
                throw new CmsV2ApplicationException("HandoutVersionItem target only allows SectionVariant or ContentBlock.");
            }
        }

        return sources;
    }

    private async Task ResolveSectionVariantAsync(
        int sectionVariantId,
        List<ResolvedHandoutSource> sources,
        CancellationToken cancellationToken)
    {
        if (await _unitOfWork.SectionVariants.GetByIdAsync(sectionVariantId, cancellationToken) is null)
        {
            throw new CmsV2ApplicationException($"SectionVariant {sectionVariantId} was not found.");
        }

        var variantItems = await _unitOfWork.SectionVariantItems.ListBySectionVariantAsync(
            sectionVariantId,
            cancellationToken);

        foreach (var variantItem in variantItems)
        {
            var sectionItem = await _unitOfWork.SectionItems.GetByIdAsync(
                variantItem.SectionItemId,
                cancellationToken);
            if (sectionItem is null)
            {
                throw new CmsV2ApplicationException($"SectionItem {variantItem.SectionItemId} was not found.");
            }

            await ResolveSectionItemAsync(sectionItem, sources, cancellationToken);
        }
    }

    private async Task ResolveSectionItemAsync(
        SectionItem sectionItem,
        List<ResolvedHandoutSource> sources,
        CancellationToken cancellationToken)
    {
        if (sectionItem.TargetType == SectionItemTargetType.ContentBlock)
        {
            var lockedVersionId = sectionItem.ReferenceMode == ReferenceMode.LockedVersion
                ? sectionItem.LockedContentBlockVersionId
                : null;

            await ResolveContentBlockTreeAsync(
                sectionItem.TargetId,
                lockedVersionId,
                sectionItem.TitleOverride,
                sources,
                new HashSet<int>(),
                depth: 1,
                cancellationToken);

            return;
        }

        if (sectionItem.TargetType != SectionItemTargetType.AtomicSection)
        {
            throw new CmsV2ApplicationException("SectionItem target only allows ContentBlock or AtomicSection.");
        }

        if (await _unitOfWork.AtomicSections.GetByIdAsync(sectionItem.TargetId, cancellationToken) is null)
        {
            throw new CmsV2ApplicationException($"AtomicSection {sectionItem.TargetId} was not found.");
        }

        var atomicItems = await _unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(
            sectionItem.TargetId,
            cancellationToken);

        foreach (var atomicItem in atomicItems)
        {
            var lockedVersionId = atomicItem.ReferenceMode == ReferenceMode.LockedVersion
                ? atomicItem.LockedContentBlockVersionId
                : null;

            await ResolveContentBlockTreeAsync(
                atomicItem.ContentBlockId,
                lockedVersionId,
                atomicItem.TitleOverride,
                sources,
                new HashSet<int>(),
                depth: 1,
                cancellationToken);
        }
    }

    private async Task ResolveContentBlockTreeAsync(
        int contentBlockId,
        int? lockedContentBlockVersionId,
        string? titleOverride,
        List<ResolvedHandoutSource> sources,
        HashSet<int> currentPath,
        int depth,
        CancellationToken cancellationToken)
    {
        if (depth > MaxContentBlockExpandDepth)
        {
            throw new CmsV2ApplicationException($"ContentBlock nesting depth cannot exceed {MaxContentBlockExpandDepth}.");
        }

        if (!currentPath.Add(contentBlockId))
        {
            throw new CmsV2ApplicationException("Recursive ContentBlock relation was detected.");
        }

        try
        {
            var source = await ResolveContentBlockSourceAsync(
                contentBlockId,
                lockedContentBlockVersionId,
                titleOverride,
                sources.Count + 1,
                cancellationToken);
            sources.Add(source);

            var childRelations = await _unitOfWork.ContentBlockRelations.ListChildrenAsync(
                contentBlockId,
                cancellationToken);

            foreach (var relation in childRelations)
            {
                var childLockedVersionId = relation.ReferenceMode == ReferenceMode.LockedVersion
                    ? relation.LockedContentBlockVersionId
                    : null;

                await ResolveContentBlockTreeAsync(
                    relation.ChildBlockId,
                    childLockedVersionId,
                    relation.TitleOverride,
                    sources,
                    currentPath,
                    depth + 1,
                    cancellationToken);
            }
        }
        finally
        {
            currentPath.Remove(contentBlockId);
        }
    }

    private async Task<ResolvedHandoutSource> ResolveContentBlockSourceAsync(
        int contentBlockId,
        int? lockedContentBlockVersionId,
        string? titleOverride,
        int sequence,
        CancellationToken cancellationToken)
    {
        var contentBlock = await _unitOfWork.ContentBlocks.GetByIdAsync(contentBlockId, cancellationToken)
            ?? throw new CmsV2ApplicationException($"ContentBlock {contentBlockId} was not found.");
        ContentBlockVersion? version;

        if (lockedContentBlockVersionId.HasValue)
        {
            version = await _unitOfWork.ContentBlockVersions.GetByIdAsync(
                lockedContentBlockVersionId.Value,
                cancellationToken);
            if (version is null)
            {
                throw new CmsV2ApplicationException($"ContentBlockVersion {lockedContentBlockVersionId.Value} was not found.");
            }

            if (version.ContentBlockId != contentBlockId)
            {
                throw new CmsV2ApplicationException("Locked ContentBlockVersion does not belong to the referenced ContentBlock.");
            }
        }
        else
        {
            version = await _unitOfWork.ContentBlockVersions.GetCurrentByContentBlockAsync(
                contentBlockId,
                cancellationToken);
            if (version is null)
            {
                throw new CmsV2ApplicationException($"ContentBlock {contentBlockId} does not have a current version.");
            }
        }

        if (!await _fileStore.ExistsAsync(version.DocxPath, cancellationToken))
        {
            throw new CmsV2ApplicationException($"ContentBlockVersion DOCX file was not found: {version.DocxPath}");
        }

        return new ResolvedHandoutSource(
            sequence,
            contentBlock.Id,
            version.Id,
            version.VersionNumber,
            string.IsNullOrWhiteSpace(titleOverride) ? contentBlock.Title : titleOverride.Trim(),
            version.DocxPath);
    }

    private async Task<OutputForm> RequireOutputFormAsync(int outputFormId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.OutputForms.GetByIdAsync(outputFormId, cancellationToken)
            ?? throw new CmsV2ApplicationException($"OutputForm {outputFormId} was not found.");
    }

    private async Task<HandoutVersion> RequireHandoutVersionAsync(int handoutVersionId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.HandoutVersions.GetByIdAsync(handoutVersionId, cancellationToken)
            ?? throw new CmsV2ApplicationException($"HandoutVersion {handoutVersionId} was not found.");
    }

    private async Task<OutputTemplate> RequireOutputTemplateAsync(int outputTemplateId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.OutputTemplates.GetByIdAsync(outputTemplateId, cancellationToken)
            ?? throw new CmsV2ApplicationException($"OutputTemplate {outputTemplateId} was not found.");
    }

    private async Task CleanupOutputFileAsync(string? outputDocxPath)
    {
        if (string.IsNullOrWhiteSpace(outputDocxPath))
        {
            return;
        }

        try
        {
            await _fileStore.DeleteIfExistsAsync(outputDocxPath, CancellationToken.None);
        }
        catch
        {
            // Best-effort cleanup only; preserve the original generation failure.
        }
    }

    private static string CreateVersionManifestJson(
        int outputFormId,
        int handoutVersionId,
        DateTimeOffset generatedTime,
        IReadOnlyList<ResolvedHandoutSource> sources)
    {
        var manifest = new VersionManifest(
            SchemaVersion: 1,
            GeneratedTime: generatedTime.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture),
            outputFormId,
            handoutVersionId,
            sources.Select(source => new VersionManifestSource(
                source.Sequence,
                source.ContentBlockId,
                source.ContentBlockVersionId,
                source.VersionNumber,
                source.DocxPath)).ToArray());

        return JsonSerializer.Serialize(
            manifest,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
    }

    private sealed record ResolvedHandoutSource(
        int Sequence,
        int ContentBlockId,
        int ContentBlockVersionId,
        int VersionNumber,
        string Title,
        string DocxPath);

    private sealed record VersionManifest(
        int SchemaVersion,
        string GeneratedTime,
        int OutputFormId,
        int HandoutVersionId,
        IReadOnlyList<VersionManifestSource> Sources);

    private sealed record VersionManifestSource(
        int Sequence,
        int ContentBlockId,
        int ContentBlockVersionId,
        int VersionNumber,
        string DocxPath);
}
