using Aspose.Words;
using Aspose.Words.Lists;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Infrastructure.Documents;

public sealed class AsposeHandoutDocumentGenerator : IHandoutDocumentGenerator
{
    public Task<IReadOnlyList<HandoutDocumentGenerationIssue>> ValidateWordGenerationAsync(
        string templateDocxPath,
        IReadOnlyList<HandoutDocumentElement> elements,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(templateDocxPath, nameof(templateDocxPath));
        ArgumentNullException.ThrowIfNull(elements);

        return Task.Run<IReadOnlyList<HandoutDocumentGenerationIssue>>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var outputDocument = AsposeTemplateDocumentFactory.CreateDocumentCopy(
                    templateDocxPath,
                    TemplateHeaderFooterMode.Preserve);
                return ValidateWordGeneration(outputDocument, elements);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                return
                [
                    new HandoutDocumentGenerationIssue(
                        "UnreadableOutputTemplate",
                        $"OutputTemplate DOCX file could not be read: {templateDocxPath}",
                        Severity: HandoutDocumentGenerationIssueSeverity.Blocking)
                ];
            }
        }, cancellationToken);
    }

    public Task GenerateWordAsync(
        string handoutTitle,
        string templateDocxPath,
        IReadOnlyList<HandoutDocumentSource> sources,
        string outputDocxPath,
        DateTimeOffset generatedTime,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sources);

        var elements = sources
            .Select(source => HandoutDocumentElement.ContentBlock(source.Title, source.DocxPath))
            .ToArray();

        return GenerateWordAsync(
            handoutTitle,
            templateDocxPath,
            elements,
            outputDocxPath,
            generatedTime,
            HandoutDocumentGenerationOptions.Default,
            cancellationToken);
    }

    public Task GenerateWordAsync(
        string handoutTitle,
        string templateDocxPath,
        IReadOnlyList<HandoutDocumentElement> elements,
        string outputDocxPath,
        DateTimeOffset generatedTime,
        CancellationToken cancellationToken = default)
    {
        return GenerateWordAsync(
            handoutTitle,
            templateDocxPath,
            elements,
            outputDocxPath,
            generatedTime,
            HandoutDocumentGenerationOptions.Default,
            cancellationToken);
    }

    public Task GenerateWordAsync(
        string handoutTitle,
        string templateDocxPath,
        IReadOnlyList<HandoutDocumentElement> elements,
        string outputDocxPath,
        DateTimeOffset generatedTime,
        HandoutDocumentGenerationOptions options,
        CancellationToken cancellationToken = default)
    {
        ValidateText(handoutTitle, nameof(handoutTitle));
        ValidatePath(templateDocxPath, nameof(templateDocxPath));
        ArgumentNullException.ThrowIfNull(elements);
        ValidatePath(outputDocxPath, nameof(outputDocxPath));
        ArgumentNullException.ThrowIfNull(options);

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureParentDirectory(outputDocxPath);

            var outputDocument = AsposeTemplateDocumentFactory.CreateDocumentCopy(
                templateDocxPath,
                TemplateHeaderFooterMode.Preserve);
            var issues = ValidateWordGeneration(outputDocument, elements);
            ThrowIfValidationIssues(issues);
            var builder = new DocumentBuilder(outputDocument);

            builder.MoveToDocumentEnd();
            if (options.IncludeDocumentTitle)
            {
                builder.InsertBreak(BreakType.ParagraphBreak);
                builder.Font.Name = "Microsoft YaHei";
                builder.Font.Size = 16;
                builder.Font.Bold = true;
                builder.Writeln(handoutTitle.Trim());
            }

            if (options.IncludeGeneratedTime)
            {
                builder.Font.Name = "Microsoft YaHei";
                builder.Font.Bold = false;
                builder.Font.Size = 10;
                builder.Writeln($"Generated: {generatedTime:yyyy-MM-dd HH:mm}");
            }

            builder.Font.Size = 11;

            if (elements.Count == 0 && options.IncludeEmptyContentPlaceholder)
            {
                builder.Writeln("No content.");
            }

            foreach (var element in elements)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (element.Kind == HandoutDocumentElementKind.Heading)
                {
                    ValidateText(element.Title, nameof(element.Title));
                    WriteHeading(builder, element.Title, element.HeadingLevel);
                    continue;
                }

                if (element.Kind != HandoutDocumentElementKind.ContentBlock)
                {
                    throw new ArgumentException($"Unsupported handout document element kind: {element.Kind}.");
                }

                ValidatePath(element.DocxPath ?? string.Empty, nameof(element.DocxPath));

                var sourceDocument = new Document(element.DocxPath!);
                RemoveHeadersAndFooters(sourceDocument);
                AppendBodyContent(outputDocument, sourceDocument, element.OutputStemStyleName);
            }

            RebaseTopLevelNumberedParagraphs(outputDocument);
            outputDocument.Save(outputDocxPath, SaveFormat.Docx);
        }, cancellationToken);
    }

    private static void ValidateRequiredOutputStyles(
        Document outputDocument,
        IReadOnlyList<HandoutDocumentElement> elements)
    {
        ThrowIfValidationIssues(ValidateRequiredOutputStylesAsIssues(outputDocument, elements));
    }

    private static IReadOnlyList<HandoutDocumentGenerationIssue> ValidateWordGeneration(
        Document outputDocument,
        IReadOnlyList<HandoutDocumentElement> elements)
    {
        return ValidateRequiredOutputStylesAsIssues(outputDocument, elements)
            .Concat(ValidateQuestionStemParagraphs(elements))
            .ToArray();
    }

    private static IReadOnlyList<HandoutDocumentGenerationIssue> ValidateRequiredOutputStylesAsIssues(
        Document outputDocument,
        IReadOnlyList<HandoutDocumentElement> elements)
    {
        var requiredStyleNames = elements
            .Where(element => element.Kind == HandoutDocumentElementKind.ContentBlock)
            .Select(element => element.OutputStemStyleName)
            .Where(styleName => !string.IsNullOrWhiteSpace(styleName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return requiredStyleNames
            .Where(styleName => FindParagraphStyle(outputDocument, styleName!) is null)
            .Select(styleName => new HandoutDocumentGenerationIssue(
                "MissingOutputStyle",
                $"OutputTemplate is missing required style '{styleName}' for question stem output.",
                Severity: HandoutDocumentGenerationIssueSeverity.Blocking,
                RequiredStyleName: styleName))
            .ToArray();
    }

    private static IReadOnlyList<HandoutDocumentGenerationIssue> ValidateQuestionStemParagraphs(
        IReadOnlyList<HandoutDocumentElement> elements)
    {
        var issues = new List<HandoutDocumentGenerationIssue>();

        foreach (var element in elements.Where(element =>
            element.Kind == HandoutDocumentElementKind.ContentBlock
            && !string.IsNullOrWhiteSpace(element.OutputStemStyleName)))
        {
            Document sourceDocument;
            try
            {
                ValidatePath(element.DocxPath ?? string.Empty, nameof(element.DocxPath));
                sourceDocument = new Document(element.DocxPath!);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                issues.Add(new HandoutDocumentGenerationIssue(
                    "UnreadableContentBlockDocx",
                    $"ContentBlockVersion DOCX file could not be read: {element.DocxPath}",
                    Severity: HandoutDocumentGenerationIssueSeverity.WarningSkip,
                    ContentBlockId: element.ContentBlockId,
                    ContentBlockVersionId: element.ContentBlockVersionId,
                    RequiredStyleName: element.OutputStemStyleName,
                    OccurrenceRole: element.OccurrenceRole));
                continue;
            }

            if (ContainsEffectiveStemParagraph(sourceDocument))
            {
                continue;
            }

            issues.Add(new HandoutDocumentGenerationIssue(
                "MissingQuestionStem",
                $"Question ContentBlock {element.ContentBlockId?.ToString() ?? element.Title} does not contain an effective Stem paragraph.",
                Severity: HandoutDocumentGenerationIssueSeverity.WarningSkip,
                ContentBlockId: element.ContentBlockId,
                ContentBlockVersionId: element.ContentBlockVersionId,
                RequiredStyleName: element.OutputStemStyleName,
                OccurrenceRole: element.OccurrenceRole));
        }

        return issues;
    }

    private static void ThrowIfValidationIssues(IReadOnlyList<HandoutDocumentGenerationIssue> issues)
    {
        if (issues.Count == 0)
        {
            return;
        }

        throw new HandoutDocumentGenerationException(
            string.Join(Environment.NewLine, issues.Select(issue => $"{issue.Code}: {issue.Message}")));
    }

    private static void AppendBodyContent(
        Document outputDocument,
        Document sourceDocument,
        string? outputStemStyleName)
    {
        var importer = new NodeImporter(
            sourceDocument,
            outputDocument,
            ImportFormatMode.KeepSourceFormatting);
        var targetBody = outputDocument.LastSection.Body;
        var outputStyle = string.IsNullOrWhiteSpace(outputStemStyleName)
            ? null
            : FindParagraphStyle(outputDocument, outputStemStyleName)
                ?? throw new HandoutDocumentGenerationException(
                    $"OutputTemplate is missing required style '{outputStemStyleName}' for question stem output.");
        var stemStyleRebound = false;

        foreach (Section sourceSection in sourceDocument.Sections)
        {
            foreach (Node sourceNode in sourceSection.Body.GetChildNodes(NodeType.Any, isDeep: false))
            {
                var importedNode = importer.ImportNode(sourceNode, isImportChildren: true);
                targetBody.AppendChild(importedNode);

                if (outputStyle is not null && !stemStyleRebound)
                {
                    stemStyleRebound = TryRebindImportedStemParagraph(sourceNode, importedNode, outputStyle);
                }
            }
        }

        if (outputStyle is not null && !stemStyleRebound)
        {
            throw new HandoutDocumentGenerationException(
                $"MissingQuestionStem: question source does not contain an effective Stem paragraph for output style '{outputStemStyleName}'.");
        }
    }

    private static bool TryRebindImportedStemParagraph(Node sourceNode, Node importedNode, Style outputStyle)
    {
        var sourceParagraphs = GetImportedParagraphs(sourceNode).ToArray();
        var stemIndex = Array.FindIndex(sourceParagraphs, IsEffectiveStemParagraph);
        if (stemIndex < 0)
        {
            return false;
        }

        var importedParagraphs = GetImportedParagraphs(importedNode).ToArray();
        if (stemIndex >= importedParagraphs.Length)
        {
            return false;
        }

        RebindParagraphStyle(importedParagraphs[stemIndex], outputStyle);
        return true;
    }

    private static void RebindParagraphStyle(Paragraph paragraph, Style outputStyle)
    {
        paragraph.ParagraphFormat.ClearFormatting();
        paragraph.ParagraphFormat.Style = outputStyle;
        paragraph.ParagraphFormat.StyleName = outputStyle.Name;
        foreach (Run run in paragraph.GetChildNodes(NodeType.Run, true).OfType<Run>())
        {
            run.Font.ClearFormatting();
        }
    }

    private static IEnumerable<Paragraph> GetImportedParagraphs(Node node)
    {
        if (node is Paragraph paragraph)
        {
            yield return paragraph;
        }

        if (node is not CompositeNode compositeNode)
        {
            yield break;
        }

        foreach (var childParagraph in compositeNode.GetChildNodes(NodeType.Paragraph, true).OfType<Paragraph>())
        {
            yield return childParagraph;
        }
    }

    private static bool IsEffectiveStemParagraph(Paragraph paragraph)
    {
        var text = paragraph.GetText();
        if (string.IsNullOrWhiteSpace(text) || IsAsposeEvaluationParagraph(text))
        {
            return false;
        }

        var partType = ResolveParagraphPartType(paragraph);
        return partType is ContentBlockPartType.Stem;
    }

    private static bool ContainsEffectiveStemParagraph(Document document)
    {
        return document
            .GetChildNodes(NodeType.Paragraph, true)
            .OfType<Paragraph>()
            .Any(IsEffectiveStemParagraph);
    }

    private static ContentBlockPartType ResolveParagraphPartType(Paragraph paragraph)
    {
        var styleNamePartType = QuestionPartStyleOptions.Default.ResolvePartType(paragraph.ParagraphFormat.StyleName);
        if (styleNamePartType != ContentBlockPartType.Other)
        {
            return styleNamePartType;
        }

        return QuestionPartStyleOptions.Default.ResolvePartType(paragraph.ParagraphFormat.Style?.Name);
    }

    private static bool IsAsposeEvaluationParagraph(string text)
    {
        return text.Contains("Created with an evaluation copy of Aspose.Words", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Evaluation Only. Created with Aspose.Words", StringComparison.OrdinalIgnoreCase);
    }

    private static Style? FindParagraphStyle(Document document, string styleName)
    {
        return document.Styles
            .OfType<Style>()
            .FirstOrDefault(style =>
                style.Type == StyleType.Paragraph
                && string.Equals(style.Name, styleName.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static void WriteHeading(DocumentBuilder builder, string title, int headingLevel)
    {
        if (headingLevel is < 1 or > 9)
        {
            throw new ArgumentOutOfRangeException(nameof(headingLevel), "Heading level must be between 1 and 9.");
        }

        builder.MoveToDocumentEnd();
        builder.InsertBreak(BreakType.ParagraphBreak);
        builder.ParagraphFormat.ClearFormatting();
        builder.Font.ClearFormatting();
        builder.ParagraphFormat.StyleIdentifier = GetHeadingStyleIdentifier(headingLevel);
        builder.Writeln(title.Trim());
        builder.ParagraphFormat.ClearFormatting();
        builder.Font.ClearFormatting();
    }

    private static StyleIdentifier GetHeadingStyleIdentifier(int headingLevel)
    {
        return headingLevel switch
        {
            1 => StyleIdentifier.Heading1,
            2 => StyleIdentifier.Heading2,
            3 => StyleIdentifier.Heading3,
            4 => StyleIdentifier.Heading4,
            5 => StyleIdentifier.Heading5,
            6 => StyleIdentifier.Heading6,
            7 => StyleIdentifier.Heading7,
            8 => StyleIdentifier.Heading8,
            _ => StyleIdentifier.Heading9
        };
    }

    private static void RebaseTopLevelNumberedParagraphs(Document document)
    {
        var numberedParagraphs = document
            .GetChildNodes(NodeType.Paragraph, true)
            .OfType<Paragraph>()
            .Where(paragraph => paragraph.IsListItem && paragraph.ListFormat.ListLevelNumber == 0)
            .ToArray();

        if (numberedParagraphs.Length == 0)
        {
            return;
        }

        var sharedList = document.Lists.Add(ListTemplate.NumberDefault);
        foreach (var paragraph in numberedParagraphs)
        {
            paragraph.ListFormat.List = sharedList;
            paragraph.ListFormat.ListLevelNumber = 0;
        }

        document.UpdateListLabels();
    }

    private static void RemoveHeadersAndFooters(Document document)
    {
        foreach (Section section in document.Sections)
        {
            section.HeadersFooters.Clear();
        }
    }

    private static void EnsureParentDirectory(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static void ValidatePath(string filePath, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty.", parameterName);
        }
    }

    private static void ValidateText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }
    }
}
