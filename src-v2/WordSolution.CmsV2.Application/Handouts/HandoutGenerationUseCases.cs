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
    private const string WordDocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    private static readonly QuestionOutputStyleOptions QuestionOutputStyles = QuestionOutputStyleOptions.Default;

    private readonly ICmsV2UnitOfWork _unitOfWork;
    private readonly ICmsV2FileAssetPathProvider _pathProvider;
    private readonly IContentBlockFileStore _fileStore;
    private readonly IOutputTemplatePathResolver _outputTemplatePathResolver;
    private readonly IHandoutDocumentGenerator _documentGenerator;

    public HandoutGenerationUseCases(
        ICmsV2UnitOfWork unitOfWork,
        ICmsV2FileAssetPathProvider pathProvider,
        IContentBlockFileStore fileStore,
        IOutputTemplatePathResolver outputTemplatePathResolver,
        IHandoutDocumentGenerator documentGenerator)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
        _outputTemplatePathResolver = outputTemplatePathResolver ?? throw new ArgumentNullException(nameof(outputTemplatePathResolver));
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
                var context = await ResolveHandoutWordGenerationContextAsync(
                    command.OutputFormId,
                    transactionCancellationToken);
                var issues = await ValidateResolvedHandoutWordGenerationAsync(context, transactionCancellationToken);
                var blockingIssues = GetBlockingIssues(issues);
                if (blockingIssues.Count > 0)
                {
                    throw new CmsV2ApplicationException(CreateValidationFailureMessage(blockingIssues));
                }

                var generationContent = RemoveSkippedContentBlocks(
                    context.ResolvedContent,
                    issues);

                outputDocxPath = _pathProvider.GetGeneratedHandoutDocxPath(
                    command.BankRootDirectory,
                    context.HandoutVersion.Id,
                    context.OutputForm.Id,
                    context.OutputForm.Title,
                    generatedTime);

                await _documentGenerator.GenerateWordAsync(
                    context.HandoutVersion.Title,
                    context.ResolvedTemplateDocxPath,
                    generationContent.Elements,
                    outputDocxPath,
                    generatedTime,
                    transactionCancellationToken);

                var manifestJson = CreateVersionManifestJson(
                    context.OutputForm.Id,
                    context.HandoutVersion.Id,
                    generatedTime,
                    generationContent.Sources);
                var generatedFile = new GeneratedFile(
                    context.OutputForm.Id,
                    outputDocxPath,
                    manifestJson,
                    generatedTime);

                await _unitOfWork.GeneratedFiles.AddAsync(generatedFile, transactionCancellationToken);
                await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

                return new GeneratedHandoutFileResult(
                    generatedFile.Id,
                    context.OutputForm.Id,
                    context.HandoutVersion.Id,
                    outputDocxPath,
                    manifestJson);
            }, cancellationToken);
        }
        catch (CmsV2ApplicationException)
        {
            await CleanupOutputFileAsync(outputDocxPath);
            throw;
        }
        catch (HandoutDocumentGenerationException exception)
        {
            await CleanupOutputFileAsync(outputDocxPath);
            throw new CmsV2ApplicationException(exception.Message, exception);
        }
        catch (Exception exception)
        {
            await CleanupOutputFileAsync(outputDocxPath);
            throw new CmsV2ApplicationException("Handout generation failed.", exception);
        }
    }

    public async Task<GeneratedSectionWordResult> GenerateSectionWordAsync(
        GenerateSectionWordCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.BankRootDirectory))
        {
            throw new CmsV2ApplicationException("BankRootDirectory cannot be empty.");
        }

        var outputDocxPath = CreateTemporarySectionDocxPath();

        try
        {
            var context = await ResolveSectionWordGenerationContextAsync(command.SectionId, cancellationToken);
            var issues = await ValidateResolvedSectionWordGenerationAsync(context, cancellationToken);
            var blockingIssues = GetBlockingIssues(issues);
            if (blockingIssues.Count > 0)
            {
                throw new CmsV2ApplicationException(CreateValidationFailureMessage(blockingIssues));
            }

            var generationContent = RemoveSkippedContentBlocks(context.ResolvedContent, issues);
            await _documentGenerator.GenerateWordAsync(
                context.Section.Title,
                context.ResolvedTemplateDocxPath,
                generationContent.Elements,
                outputDocxPath,
                DateTimeOffset.UtcNow,
                HandoutDocumentGenerationOptions.WithoutDocumentChrome,
                cancellationToken);
            var fileBytes = await _fileStore.ReadContentBlockDocxAsync(outputDocxPath, cancellationToken)
                ?? throw new CmsV2ApplicationException("Section Word file was not generated.");

            return new GeneratedSectionWordResult(
                CreateSectionWordFileName(context.Section.Title),
                WordDocxContentType,
                fileBytes,
                issues);
        }
        catch (CmsV2ApplicationException)
        {
            throw;
        }
        catch (HandoutDocumentGenerationException exception)
        {
            throw new CmsV2ApplicationException(exception.Message, exception);
        }
        catch (Exception exception)
        {
            throw new CmsV2ApplicationException("Section Word generation failed.", exception);
        }
        finally
        {
            await CleanupOutputFileAsync(outputDocxPath);
        }
    }

    public async Task<HandoutWordGenerationValidationResult> ValidateSectionWordGenerationAsync(
        ValidateSectionWordGenerationCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.BankRootDirectory))
        {
            throw new CmsV2ApplicationException("BankRootDirectory cannot be empty.");
        }

        var context = await ResolveSectionWordGenerationContextAsync(command.SectionId, cancellationToken);
        var issues = await ValidateResolvedSectionWordGenerationAsync(context, cancellationToken);

        return new HandoutWordGenerationValidationResult(GetBlockingIssues(issues).Count == 0, issues);
    }

    public async Task<HandoutWordGenerationValidationResult> ValidateHandoutWordGenerationAsync(
        ValidateHandoutWordGenerationCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.BankRootDirectory))
        {
            throw new CmsV2ApplicationException("BankRootDirectory cannot be empty.");
        }

        var context = await ResolveHandoutWordGenerationContextAsync(command.OutputFormId, cancellationToken);
        var issues = await ValidateResolvedHandoutWordGenerationAsync(context, cancellationToken);
        return new HandoutWordGenerationValidationResult(GetBlockingIssues(issues).Count == 0, issues);
    }

    private async Task<ResolvedHandoutWordGenerationContext> ResolveHandoutWordGenerationContextAsync(
        int outputFormId,
        CancellationToken cancellationToken)
    {
        var outputForm = await RequireOutputFormAsync(outputFormId, cancellationToken);
        if (outputForm.OutputFormat != OutputFormat.Word)
        {
            throw new CmsV2ApplicationException("Only Word output is supported in the current stage.");
        }

        var handoutVersion = await RequireHandoutVersionAsync(
            outputForm.HandoutVersionId,
            cancellationToken);
        var outputTemplate = await RequireOutputTemplateAsync(
            outputForm.OutputTemplateId,
            cancellationToken);

        var resolvedTemplateDocxPath = _outputTemplatePathResolver.ResolveTemplateDocxPath(outputTemplate.TemplateDocxPath);
        if (!await _fileStore.ExistsAsync(resolvedTemplateDocxPath, cancellationToken))
        {
            throw new CmsV2ApplicationException($"OutputTemplate file was not found: {outputTemplate.TemplateDocxPath}");
        }

        var resolvedContent = await ResolveHandoutContentAsync(
            handoutVersion.Id,
            cancellationToken);

        return new ResolvedHandoutWordGenerationContext(
            outputForm,
            handoutVersion,
            outputTemplate,
            resolvedTemplateDocxPath,
            resolvedContent);
    }

    private async Task<ResolvedSectionWordGenerationContext> ResolveSectionWordGenerationContextAsync(
        int sectionId,
        CancellationToken cancellationToken)
    {
        var section = await _unitOfWork.Sections.GetByIdAsync(sectionId, cancellationToken)
            ?? throw new CmsV2ApplicationException($"Section {sectionId} was not found.");
        var resolvedTemplateDocxPath = _outputTemplatePathResolver.ResolveTemplateDocxPath(
            OutputTemplatePaths.RuntimeDefaultTemplateDocxPath);
        if (!await _fileStore.ExistsAsync(resolvedTemplateDocxPath, cancellationToken))
        {
            throw new CmsV2ApplicationException(
                $"OutputTemplate file was not found: {OutputTemplatePaths.RuntimeDefaultTemplateDocxPath}");
        }

        var resolvedContent = await ResolveSectionWordContentAsync(section, cancellationToken);

        return new ResolvedSectionWordGenerationContext(
            section,
            resolvedTemplateDocxPath,
            resolvedContent);
    }

    private async Task<IReadOnlyList<HandoutDocumentGenerationIssue>> ValidateResolvedHandoutWordGenerationAsync(
        ResolvedHandoutWordGenerationContext context,
        CancellationToken cancellationToken)
    {
        var validationContent = RemoveSkippedContentBlocks(
            context.ResolvedContent,
            context.ResolvedContent.Issues);
        var documentIssues = await _documentGenerator.ValidateWordGenerationAsync(
            context.ResolvedTemplateDocxPath,
            validationContent.Elements,
            cancellationToken);

        return context.ResolvedContent.Issues
            .Concat(documentIssues)
            .Select(issue => issue with
            {
                OutputFormId = issue.OutputFormId ?? context.OutputForm.Id,
                OutputTemplateId = issue.OutputTemplateId ?? context.OutputTemplate.Id
            })
            .ToArray();
    }

    private async Task<IReadOnlyList<HandoutDocumentGenerationIssue>> ValidateResolvedSectionWordGenerationAsync(
        ResolvedSectionWordGenerationContext context,
        CancellationToken cancellationToken)
    {
        var validationContent = RemoveSkippedContentBlocks(
            context.ResolvedContent,
            context.ResolvedContent.Issues);
        var documentIssues = await _documentGenerator.ValidateWordGenerationAsync(
            context.ResolvedTemplateDocxPath,
            validationContent.Elements,
            cancellationToken);

        return context.ResolvedContent.Issues
            .Concat(documentIssues)
            .ToArray();
    }

    private static string CreateValidationFailureMessage(IReadOnlyList<HandoutDocumentGenerationIssue> issues)
    {
        return string.Join(Environment.NewLine, issues.Select(issue => $"{issue.Code}: {issue.Message}"));
    }

    private static IReadOnlyList<HandoutDocumentGenerationIssue> GetBlockingIssues(
        IReadOnlyList<HandoutDocumentGenerationIssue> issues)
    {
        return issues
            .Where(issue => issue.Severity == HandoutDocumentGenerationIssueSeverity.Blocking)
            .ToArray();
    }

    private static ResolvedHandoutContent RemoveSkippedContentBlocks(
        ResolvedHandoutContent content,
        IReadOnlyList<HandoutDocumentGenerationIssue> issues)
    {
        var skippedBlockVersions = issues
            .Where(issue => issue.Severity == HandoutDocumentGenerationIssueSeverity.WarningSkip
                && issue.ContentBlockId.HasValue
                && issue.ContentBlockVersionId.HasValue)
            .Select(issue => (ContentBlockId: issue.ContentBlockId!.Value, ContentBlockVersionId: issue.ContentBlockVersionId!.Value))
            .ToHashSet();
        var skippedContentBlocks = issues
            .Where(issue => issue.Severity == HandoutDocumentGenerationIssueSeverity.WarningSkip
                && issue.ContentBlockId.HasValue
                && !issue.ContentBlockVersionId.HasValue)
            .Select(issue => issue.ContentBlockId!.Value)
            .ToHashSet();
        if (skippedBlockVersions.Count == 0 && skippedContentBlocks.Count == 0)
        {
            return content;
        }

        var sources = content.Sources
            .Where(source => !skippedBlockVersions.Contains((source.ContentBlockId, source.ContentBlockVersionId)))
            .Select((source, index) => source with { Sequence = index + 1 })
            .ToArray();
        var keepElements = content.Elements
            .Select(element => !IsSkippedContentBlock(element, skippedContentBlocks, skippedBlockVersions))
            .ToArray();
        RemoveAtomicSectionHeadingsMadeEmptyByWarningSkips(content.Elements, keepElements);
        var elements = content.Elements
            .Where((_, index) => keepElements[index])
            .ToArray();

        return new ResolvedHandoutContent(sources, elements, content.Issues);
    }

    private static bool IsSkippedContentBlock(
        HandoutDocumentElement element,
        HashSet<int> skippedContentBlocks,
        HashSet<(int ContentBlockId, int ContentBlockVersionId)> skippedBlockVersions)
    {
        if (element.Kind != HandoutDocumentElementKind.ContentBlock || !element.ContentBlockId.HasValue)
        {
            return false;
        }

        return (!element.ContentBlockVersionId.HasValue && skippedContentBlocks.Contains(element.ContentBlockId.Value))
            || (element.ContentBlockVersionId.HasValue
                && skippedBlockVersions.Contains((element.ContentBlockId.Value, element.ContentBlockVersionId.Value)));
    }

    private static void RemoveAtomicSectionHeadingsMadeEmptyByWarningSkips(
        IReadOnlyList<HandoutDocumentElement> elements,
        bool[] keepElements)
    {
        for (var index = 0; index < elements.Count; index++)
        {
            var element = elements[index];
            if (element.Kind != HandoutDocumentElementKind.Heading || element.HeadingLevel != 3)
            {
                continue;
            }

            var hasSkippedContent = false;
            var hasRemainingContent = false;
            for (var cursor = index + 1; cursor < elements.Count; cursor++)
            {
                var nextElement = elements[cursor];
                if (nextElement.Kind == HandoutDocumentElementKind.Heading && nextElement.HeadingLevel <= 3)
                {
                    break;
                }

                if (nextElement.Kind != HandoutDocumentElementKind.ContentBlock)
                {
                    continue;
                }

                if (keepElements[cursor])
                {
                    hasRemainingContent = true;
                    break;
                }

                hasSkippedContent = true;
            }

            if (hasSkippedContent && !hasRemainingContent)
            {
                keepElements[index] = false;
            }
        }
    }

    private async Task<ResolvedHandoutContent> ResolveHandoutContentAsync(
        int handoutVersionId,
        CancellationToken cancellationToken)
    {
        var handoutItems = await _unitOfWork.HandoutVersionItems.ListByHandoutVersionAsync(
            handoutVersionId,
            cancellationToken);
        var sources = new List<ResolvedHandoutSource>();
        var elements = new List<HandoutDocumentElement>();
        var issues = new List<HandoutDocumentGenerationIssue>();

        foreach (var item in handoutItems)
        {
            if (item.TargetType == HandoutVersionItemTargetType.ContentBlock)
            {
                await ResolveContentBlockTreeAsync(
                    item.TargetId,
                    lockedContentBlockVersionId: null,
                    item.TitleOverride,
                    requestedOutputStemStyleName: null,
                    requestedOccurrenceRole: null,
                    sources,
                    elements,
                    issues,
                    new HashSet<int>(),
                    depth: 1,
                    cancellationToken);
            }
            else if (item.TargetType == HandoutVersionItemTargetType.SectionVariant)
            {
                await ResolveSectionVariantAsync(item.TargetId, sources, elements, issues, cancellationToken);
            }
            else if (item.TargetType == HandoutVersionItemTargetType.AtomicSection)
            {
                await ResolveAtomicSectionAsync(
                    item.TargetId,
                    item.TitleOverride,
                    sources,
                    elements,
                    issues,
                    cancellationToken);
            }
            else
            {
                throw new CmsV2ApplicationException("HandoutVersionItem target only allows SectionVariant, AtomicSection or ContentBlock.");
            }
        }

        return new ResolvedHandoutContent(sources, elements, issues);
    }

    private async Task<ResolvedHandoutContent> ResolveSectionWordContentAsync(
        Section section,
        CancellationToken cancellationToken)
    {
        var sources = new List<ResolvedHandoutSource>();
        var elements = new List<HandoutDocumentElement>
        {
            HandoutDocumentElement.Heading(section.Title, headingLevel: 2)
        };
        var issues = new List<HandoutDocumentGenerationIssue>();
        var sectionItems = await _unitOfWork.SectionItems.ListBySectionAsync(
            section.Id,
            cancellationToken);

        foreach (var sectionItem in sectionItems
            .Where(item => item.ParentItemId is null)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id))
        {
            await ResolveSectionItemAsync(sectionItem, sources, elements, issues, cancellationToken);
        }

        return new ResolvedHandoutContent(sources, elements, issues);
    }

    private async Task ResolveSectionVariantAsync(
        int sectionVariantId,
        List<ResolvedHandoutSource> sources,
        List<HandoutDocumentElement> elements,
        List<HandoutDocumentGenerationIssue> issues,
        CancellationToken cancellationToken)
    {
        var variant = await _unitOfWork.SectionVariants.GetByIdAsync(sectionVariantId, cancellationToken);
        if (variant is null)
        {
            throw new CmsV2ApplicationException($"SectionVariant {sectionVariantId} was not found.");
        }

        var section = await _unitOfWork.Sections.GetByIdAsync(variant.SectionId, cancellationToken)
            ?? throw new CmsV2ApplicationException($"Section {variant.SectionId} was not found.");
        var teachingTopic = await _unitOfWork.TeachingTopics.GetByIdAsync(section.TeachingTopicId, cancellationToken)
            ?? throw new CmsV2ApplicationException($"TeachingTopic {section.TeachingTopicId} was not found.");

        elements.Add(HandoutDocumentElement.Heading(teachingTopic.Name, headingLevel: 1));
        elements.Add(HandoutDocumentElement.Heading(section.Title, headingLevel: 2));

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

            await ResolveSectionItemAsync(sectionItem, sources, elements, issues, cancellationToken);
        }
    }

    private async Task ResolveSectionItemAsync(
        SectionItem sectionItem,
        List<ResolvedHandoutSource> sources,
        List<HandoutDocumentElement> elements,
        List<HandoutDocumentGenerationIssue> issues,
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
                requestedOutputStemStyleName: null,
                requestedOccurrenceRole: null,
                sources,
                elements,
                issues,
                new HashSet<int>(),
                depth: 1,
                cancellationToken);

            return;
        }

        if (sectionItem.TargetType != SectionItemTargetType.AtomicSection)
        {
            throw new CmsV2ApplicationException("SectionItem target only allows ContentBlock or AtomicSection.");
        }

        await ResolveAtomicSectionAsync(
            sectionItem.TargetId,
            sectionItem.TitleOverride,
            sources,
            elements,
            issues,
            cancellationToken);
    }

    private async Task ResolveAtomicSectionAsync(
        int atomicSectionId,
        string? titleOverride,
        List<ResolvedHandoutSource> sources,
        List<HandoutDocumentElement> elements,
        List<HandoutDocumentGenerationIssue> issues,
        CancellationToken cancellationToken)
    {
        var atomicSection = await _unitOfWork.AtomicSections.GetByIdAsync(atomicSectionId, cancellationToken)
            ?? throw new CmsV2ApplicationException($"AtomicSection {atomicSectionId} was not found.");
        var atomicSectionTitle = string.IsNullOrWhiteSpace(titleOverride)
            ? atomicSection.Title
            : titleOverride.Trim();
        elements.Add(HandoutDocumentElement.Heading(atomicSectionTitle, headingLevel: 3));

        var atomicItems = await _unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(
            atomicSectionId,
            cancellationToken);
        var panels = await _unitOfWork.AtomicSectionPanels.ListByAtomicSectionAsync(
            atomicSectionId,
            cancellationToken);

        foreach (var (atomicItem, teachingRole) in OrderAtomicSectionItemsForOutput(atomicItems, panels))
        {
            var lockedVersionId = atomicItem.ReferenceMode == ReferenceMode.LockedVersion
                ? atomicItem.LockedContentBlockVersionId
                : null;
            var outputStemStyleName = QuestionOutputStyles.ResolveForTeachingRole(teachingRole);
            if (teachingRole == AtomicSectionTeachingRole.PreClassQuiz && outputStemStyleName is null)
            {
                continue;
            }

            await ResolveContentBlockTreeAsync(
                atomicItem.ContentBlockId,
                lockedVersionId,
                atomicItem.TitleOverride,
                outputStemStyleName,
                teachingRole.ToString(),
                sources,
                elements,
                issues,
                new HashSet<int>(),
                depth: 1,
                cancellationToken);
        }
    }

    private static IEnumerable<(AtomicSectionItem Item, AtomicSectionTeachingRole TeachingRole)> OrderAtomicSectionItemsForOutput(
        IReadOnlyList<AtomicSectionItem> atomicItems,
        IReadOnlyList<AtomicSectionPanel> panels)
    {
        foreach (var panel in panels.OrderBy(panel => panel.SortOrder).ThenBy(panel => panel.Id))
        {
            foreach (var item in atomicItems
                .Where(item => item.AtomicSectionPanelId == panel.Id)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id))
            {
                yield return (item, item.TeachingRole == AtomicSectionTeachingRole.Unclassified
                    ? panel.TeachingRole
                    : item.TeachingRole);
            }
        }

        foreach (var item in atomicItems
            .Where(item => item.AtomicSectionPanelId is null)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id))
        {
            yield return (item, item.TeachingRole);
        }
    }

    private async Task ResolveContentBlockTreeAsync(
        int contentBlockId,
        int? lockedContentBlockVersionId,
        string? titleOverride,
        string? requestedOutputStemStyleName,
        string? requestedOccurrenceRole,
        List<ResolvedHandoutSource> sources,
        List<HandoutDocumentElement> elements,
        List<HandoutDocumentGenerationIssue> issues,
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
            var reference = await ResolveContentBlockReferenceAsync(
                contentBlockId,
                lockedContentBlockVersionId,
                titleOverride,
                requestedOutputStemStyleName,
                requestedOccurrenceRole,
                sources.Count + 1,
                issues,
                cancellationToken);
            if (reference.Source is not null)
            {
                sources.Add(reference.Source);
            }

            elements.Add(reference.Element);
            if (!reference.ShouldExpandChildren)
            {
                return;
            }

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
                    QuestionOutputStyles.PracticeStemStyleName,
                    nameof(AtomicSectionTeachingRole.Practice),
                    sources,
                    elements,
                    issues,
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

    private async Task<ResolvedContentBlockReference> ResolveContentBlockReferenceAsync(
        int contentBlockId,
        int? lockedContentBlockVersionId,
        string? titleOverride,
        string? requestedOutputStemStyleName,
        string? requestedOccurrenceRole,
        int sequence,
        List<HandoutDocumentGenerationIssue> issues,
        CancellationToken cancellationToken)
    {
        var contentBlock = await _unitOfWork.ContentBlocks.GetByIdAsync(contentBlockId, cancellationToken)
            ?? throw new CmsV2ApplicationException($"ContentBlock {contentBlockId} was not found.");
        var title = string.IsNullOrWhiteSpace(titleOverride) ? contentBlock.Title : titleOverride.Trim();
        var outputStemStyle = ResolveOutputStemStyle(
            contentBlock.BlockType,
            requestedOutputStemStyleName,
            requestedOccurrenceRole);
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
                issues.Add(new HandoutDocumentGenerationIssue(
                    "MissingContentBlockCurrentVersion",
                    $"ContentBlock {contentBlockId} does not have a current version.",
                    Severity: HandoutDocumentGenerationIssueSeverity.WarningSkip,
                    ContentBlockId: contentBlock.Id,
                    RequiredStyleName: outputStemStyle.StyleName,
                    OccurrenceRole: outputStemStyle.OccurrenceRole));

                return new ResolvedContentBlockReference(
                    Source: null,
                    HandoutDocumentElement.ContentBlock(
                        title,
                        string.Empty,
                        outputStemStyle.StyleName,
                        contentBlock.Id,
                        null,
                        outputStemStyle.OccurrenceRole),
                    ShouldExpandChildren: false);
            }
        }

        var source = new ResolvedHandoutSource(
            sequence,
            contentBlock.Id,
            version.Id,
            version.VersionNumber,
            title,
            version.DocxPath,
            outputStemStyle.StyleName,
            outputStemStyle.OccurrenceRole);
        var element = HandoutDocumentElement.ContentBlock(
            source.Title,
            source.DocxPath,
            source.OutputStemStyleName,
            source.ContentBlockId,
            source.ContentBlockVersionId,
            source.OccurrenceRole);

        if (string.IsNullOrWhiteSpace(version.DocxPath)
            || !await _fileStore.ExistsAsync(version.DocxPath, cancellationToken))
        {
            issues.Add(new HandoutDocumentGenerationIssue(
                "MissingContentBlockDocx",
                $"ContentBlockVersion DOCX file was not found: {version.DocxPath}",
                Severity: HandoutDocumentGenerationIssueSeverity.WarningSkip,
                ContentBlockId: contentBlock.Id,
                ContentBlockVersionId: version.Id,
                RequiredStyleName: outputStemStyle.StyleName,
                OccurrenceRole: outputStemStyle.OccurrenceRole));

            return new ResolvedContentBlockReference(source, element, ShouldExpandChildren: false);
        }

        return new ResolvedContentBlockReference(source, element, ShouldExpandChildren: true);
    }

    private static ResolvedOutputStemStyle ResolveOutputStemStyle(
        ContentBlockType contentBlockType,
        string? requestedOutputStemStyleName,
        string? requestedOccurrenceRole)
    {
        if (contentBlockType != ContentBlockType.Question)
        {
            return new ResolvedOutputStemStyle(null, null);
        }

        var styleName = string.IsNullOrWhiteSpace(requestedOutputStemStyleName)
            ? QuestionOutputStyles.PracticeStemStyleName
            : requestedOutputStemStyleName.Trim();
        var occurrenceRole = string.IsNullOrWhiteSpace(requestedOccurrenceRole)
            ? nameof(AtomicSectionTeachingRole.Practice)
            : requestedOccurrenceRole.Trim();

        return new ResolvedOutputStemStyle(styleName, occurrenceRole);
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

    private static string CreateTemporarySectionDocxPath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "cms-v2-section-word",
            Guid.NewGuid().ToString("N"),
            "section.docx");
    }

    private static string CreateSectionWordFileName(string sectionTitle)
    {
        return $"{SanitizeFileName(sectionTitle, fallback: "section")}.docx";
    }

    private static string SanitizeFileName(string fileName, string fallback)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars().ToHashSet();
        var sanitized = new string(fileName
            .Trim()
            .Select(character => invalidCharacters.Contains(character) ? '_' : character)
            .ToArray())
            .Trim();

        return string.IsNullOrWhiteSpace(sanitized)
            ? fallback
            : sanitized;
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
        string DocxPath,
        string? OutputStemStyleName,
        string? OccurrenceRole);

    private sealed record ResolvedHandoutContent(
        IReadOnlyList<ResolvedHandoutSource> Sources,
        IReadOnlyList<HandoutDocumentElement> Elements,
        IReadOnlyList<HandoutDocumentGenerationIssue> Issues);

    private sealed record ResolvedContentBlockReference(
        ResolvedHandoutSource? Source,
        HandoutDocumentElement Element,
        bool ShouldExpandChildren);

    private sealed record ResolvedHandoutWordGenerationContext(
        OutputForm OutputForm,
        HandoutVersion HandoutVersion,
        OutputTemplate OutputTemplate,
        string ResolvedTemplateDocxPath,
        ResolvedHandoutContent ResolvedContent);

    private sealed record ResolvedSectionWordGenerationContext(
        Section Section,
        string ResolvedTemplateDocxPath,
        ResolvedHandoutContent ResolvedContent);

    private sealed record ResolvedOutputStemStyle(
        string? StyleName,
        string? OccurrenceRole);

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
