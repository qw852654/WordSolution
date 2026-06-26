using Aspose.Words;
using Aspose.Words.Saving;
using Aspose.Words.Tables;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Infrastructure.Documents;

public sealed class AsposeContentBlockDocumentProcessor : IContentBlockDocumentProcessor
{
    private static readonly ContentBlockPartType[] QuestionPartSystemOrder =
    [
        ContentBlockPartType.Stem,
        ContentBlockPartType.Answer,
        ContentBlockPartType.Analysis,
        ContentBlockPartType.Hint,
        ContentBlockPartType.Other
    ];

    private readonly string _templateDocxPath;
    private readonly QuestionPartStyleOptions _questionPartStyleOptions;

    public AsposeContentBlockDocumentProcessor()
        : this(AsposeTemplateDocumentFactory.ResolveDefaultTemplateDocxPath(), QuestionPartStyleOptions.Default)
    {
    }

    public AsposeContentBlockDocumentProcessor(string templateDocxPath)
        : this(templateDocxPath, QuestionPartStyleOptions.Default)
    {
    }

    public AsposeContentBlockDocumentProcessor(
        string templateDocxPath,
        QuestionPartStyleOptions questionPartStyleOptions)
    {
        ValidatePath(templateDocxPath, nameof(templateDocxPath));
        _templateDocxPath = Path.GetFullPath(templateDocxPath);
        _questionPartStyleOptions = questionPartStyleOptions
            ?? throw new ArgumentNullException(nameof(questionPartStyleOptions));
    }

    public Task CreateBlankDocxAsync(
        string docxPath,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(docxPath, nameof(docxPath));

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureParentDirectory(docxPath);

            AsposeTemplateDocumentFactory.CopyTemplateTo(_templateDocxPath, docxPath);
        }, cancellationToken);
    }

    public Task GenerateHtmlPreviewAsync(
        string docxPath,
        string htmlPreviewPath,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(docxPath, nameof(docxPath));
        ValidatePath(htmlPreviewPath, nameof(htmlPreviewPath));

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureParentDirectory(htmlPreviewPath);

            var document = new Document(docxPath);
            RemoveHeadersAndFooters(document);
            var saveOptions = new HtmlSaveOptions
            {
                ExportImagesAsBase64 = true,
                ExportListLabels = ExportListLabels.AsInlineText,
                PrettyFormat = true
            };

            document.Save(htmlPreviewPath, saveOptions);
        }, cancellationToken);
    }

    public Task<string> ExtractPlainTextAsync(
        string docxPath,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(docxPath, nameof(docxPath));

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var document = new Document(docxPath);
            RemoveHeadersAndFooters(document);

            return document.ToString(SaveFormat.Text).Trim();
        }, cancellationToken);
    }

    public Task<QuestionPartParseResult> GenerateQuestionPartHtmlPreviewAsync(
        string docxPath,
        string htmlPreviewPath,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(docxPath, nameof(docxPath));
        ValidatePath(htmlPreviewPath, nameof(htmlPreviewPath));

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureParentDirectory(htmlPreviewPath);

            var document = new Document(docxPath);
            RemoveHeadersAndFooters(document);

            var classifiedNodes = ClassifyTopLevelNodes(document);
            var groupedNodesByPartType = classifiedNodes
                .GroupBy(node => node.PartType)
                .ToDictionary(group => group.Key, group => group.OrderBy(node => node.SourceOrder).ToList());

            if (!groupedNodesByPartType.TryGetValue(ContentBlockPartType.Stem, out var stemNodes)
                || !stemNodes.Any(node => HasEffectiveStemContent(node.Node)))
            {
                return new QuestionPartParseResult(
                    ContentBlockPartParseStatus.Failed,
                    "Question part parsing failed: Stem part was not found.",
                    []);
            }

            var parts = new List<QuestionPartParseResultItem>();
            var sectionHtml = new List<string>();
            var sortOrder = 0;
            var warnings = new List<string>();

            foreach (var partType in QuestionPartSystemOrder)
            {
                if (!groupedNodesByPartType.TryGetValue(partType, out var nodes))
                {
                    continue;
                }

                var warning = string.Join(
                    Environment.NewLine,
                    nodes.Select(node => node.WarningMessage).Where(message => !string.IsNullOrWhiteSpace(message)));
                if (!string.IsNullOrWhiteSpace(warning))
                {
                    warnings.Add(warning);
                }

                var sourceStyleNames = nodes
                    .SelectMany(node => node.SourceStyleNames)
                    .Select(QuestionPartStyleOptions.NormalizeStyleName)
                    .OfType<string>()
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(styleName => styleName, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                var plainText = string.Join(
                    Environment.NewLine,
                    nodes.Select(node => node.Node.ToString(SaveFormat.Text).TrimEnd('\r', '\n')));

                parts.Add(new QuestionPartParseResultItem(
                    partType,
                    sortOrder,
                    plainText,
                    sourceStyleNames,
                    string.IsNullOrWhiteSpace(warning) ? null : warning));

                sectionHtml.Add(BuildQuestionPartSectionHtml(
                    partType,
                    nodes.Select(node => node.Node),
                    _templateDocxPath));
                sortOrder++;
            }

            var html = BuildStructuredHtmlDocument(sectionHtml);
            File.WriteAllText(htmlPreviewPath, html);

            var status = warnings.Count == 0
                ? ContentBlockPartParseStatus.Parsed
                : ContentBlockPartParseStatus.ParsedWithWarnings;
            var message = warnings.Count == 0 ? null : string.Join(Environment.NewLine, warnings);

            return new QuestionPartParseResult(status, message, parts);
        }, cancellationToken);
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

    private IReadOnlyList<ClassifiedNode> ClassifyTopLevelNodes(Document document)
    {
        var result = new List<ClassifiedNode>();
        var body = document.FirstSection.Body;
        var sourceOrder = 0;

        var childNodes = body.GetChildNodes(NodeType.Any, false).Cast<Node>().ToArray();
        for (var nodeIndex = 0; nodeIndex < childNodes.Length; nodeIndex++)
        {
            var node = childNodes[nodeIndex];
            if (node.NodeType == NodeType.Paragraph)
            {
                var paragraph = (Paragraph)node;
                var styleName = paragraph.ParagraphFormat.StyleName;
                if (IsAsposeEvaluationParagraph(paragraph)
                    || IsTrailingGeneratedNormalParagraph(paragraph, nodeIndex, childNodes.Length))
                {
                    continue;
                }

                var partType = _questionPartStyleOptions.ResolvePartType(styleName);
                result.Add(new ClassifiedNode(
                    node,
                    partType,
                    sourceOrder,
                    [styleName],
                    partType == ContentBlockPartType.Other ? $"Unknown paragraph style: {styleName}" : null));
                sourceOrder++;
                continue;
            }

            if (node.NodeType == NodeType.Table)
            {
                result.Add(ClassifyTableNode((Table)node, sourceOrder));
                sourceOrder++;
                continue;
            }

            if (node.NodeType == NodeType.Section)
            {
                continue;
            }

            result.Add(new ClassifiedNode(
                node,
                ContentBlockPartType.Other,
                sourceOrder,
                [],
                $"Unsupported top-level node type: {node.NodeType}"));
            sourceOrder++;
        }

        return result;
    }

    private static bool IsTrailingGeneratedNormalParagraph(
        Paragraph paragraph,
        int nodeIndex,
        int nodeCount)
    {
        return nodeIndex == nodeCount - 1
            && string.Equals(paragraph.ParagraphFormat.StyleName, "Normal", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(paragraph.ToString(SaveFormat.Text));
    }

    private static bool IsAsposeEvaluationParagraph(Paragraph paragraph)
    {
        return paragraph.ToString(SaveFormat.Text)
            .TrimStart()
            .StartsWith(
                "Created with an evaluation copy of Aspose.Words.",
                StringComparison.Ordinal);
    }

    private ClassifiedNode ClassifyTableNode(Table table, int sourceOrder)
    {
        var styleNames = table.GetChildNodes(NodeType.Paragraph, true)
            .OfType<Paragraph>()
            .Select(paragraph => paragraph.ParagraphFormat.StyleName)
            .ToArray();
        var effectivePartTypes = styleNames
            .Select(_questionPartStyleOptions.ResolvePartType)
            .Distinct()
            .ToArray();

        if (effectivePartTypes.Length == 1)
        {
            var partType = effectivePartTypes[0];
            return new ClassifiedNode(
                table,
                partType,
                sourceOrder,
                styleNames,
                partType == ContentBlockPartType.Other ? "Table uses unknown paragraph styles." : null);
        }

        return new ClassifiedNode(
            table,
            ContentBlockPartType.Other,
            sourceOrder,
            styleNames,
            "Table contains mixed question part styles.");
    }

    private static string BuildQuestionPartSectionHtml(
        ContentBlockPartType partType,
        IEnumerable<Node> sourceNodes,
        string templateDocxPath)
    {
        var fragment = AsposeTemplateDocumentFactory.CreateDocumentCopy(templateDocxPath);
        var body = fragment.FirstSection.Body;
        body.RemoveAllChildren();

        foreach (var sourceNode in sourceNodes)
        {
            body.AppendChild(fragment.ImportNode(sourceNode, true));
        }

        var bodyHtml = SaveDocumentBodyHtml(fragment);
        return $"""<section data-question-part="{partType}">{bodyHtml}</section>""";
    }

    private static bool HasEffectiveStemContent(Node node)
    {
        var text = node.ToString(SaveFormat.Text);
        if (text.Any(character => !char.IsWhiteSpace(character) && !char.IsControl(character)))
        {
            return true;
        }

        if (node is not CompositeNode compositeNode)
        {
            return false;
        }

        return compositeNode.GetChildNodes(NodeType.Shape, true).Count > 0
            || compositeNode.GetChildNodes(NodeType.OfficeMath, true).Count > 0;
    }

    private static string SaveDocumentBodyHtml(Document document)
    {
        using var stream = new MemoryStream();
        var saveOptions = new HtmlSaveOptions
        {
            ExportImagesAsBase64 = true,
            ExportListLabels = ExportListLabels.AsInlineText,
            PrettyFormat = false
        };
        document.Save(stream, saveOptions);
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var html = reader.ReadToEnd();
        var bodyStart = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
        if (bodyStart < 0)
        {
            return html;
        }

        var bodyContentStart = html.IndexOf('>', bodyStart);
        var bodyEnd = html.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        if (bodyContentStart < 0 || bodyEnd < 0 || bodyEnd <= bodyContentStart)
        {
            return html;
        }

        return html[(bodyContentStart + 1)..bodyEnd].Trim();
    }

    private static string BuildStructuredHtmlDocument(IEnumerable<string> sections)
    {
        return $"""
        <!DOCTYPE html>
        <html>
        <head>
          <meta charset="utf-8" />
        </head>
        <body>
        {string.Join(Environment.NewLine, sections)}
        </body>
        </html>
        """;
    }

    private sealed record ClassifiedNode(
        Node Node,
        ContentBlockPartType PartType,
        int SourceOrder,
        IReadOnlyList<string> SourceStyleNames,
        string? WarningMessage);
}
