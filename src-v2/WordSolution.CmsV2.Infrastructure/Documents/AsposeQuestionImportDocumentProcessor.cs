using Aspose.Words;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Infrastructure.Documents;

public sealed class AsposeQuestionImportDocumentProcessor : IQuestionImportDocumentProcessor
{
    private readonly QuestionImportStyleOptions _importStyleOptions;

    public AsposeQuestionImportDocumentProcessor()
        : this(QuestionImportStyleOptions.Default)
    {
    }

    public AsposeQuestionImportDocumentProcessor(QuestionImportStyleOptions importStyleOptions)
    {
        _importStyleOptions = importStyleOptions ?? throw new ArgumentNullException(nameof(importStyleOptions));
    }

    public Task<IReadOnlyList<QuestionImportCandidateDocumentResult>> SplitCandidatesAsync(
        string sourceDocxPath,
        string candidateDirectory,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(sourceDocxPath, nameof(sourceDocxPath));
        ValidatePath(candidateDirectory, nameof(candidateDirectory));

        return Task.Run<IReadOnlyList<QuestionImportCandidateDocumentResult>>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(candidateDirectory);

            var sourceDocument = new Document(sourceDocxPath);
            var currentNodes = new List<Node>();
            var results = new List<QuestionImportCandidateDocumentResult>();

            foreach (Section section in sourceDocument.Sections)
            {
                foreach (Node node in section.Body.GetChildNodes(NodeType.Any, isDeep: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (node is Paragraph paragraph && IsQuestionStartParagraph(paragraph))
                    {
                        SaveCandidateIfAny(sourceDocument, currentNodes, candidateDirectory, results);
                        currentNodes.Clear();
                    }

                    if (currentNodes.Count > 0 || node is Paragraph startParagraph && IsQuestionStartParagraph(startParagraph))
                    {
                        currentNodes.Add(node);
                    }
                }
            }

            SaveCandidateIfAny(sourceDocument, currentNodes, candidateDirectory, results);
            return results;
        }, cancellationToken);
    }

    public Task CreateNeutralizedCandidateDocxAsync(
        string candidateDocxPath,
        string outputDocxPath,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(candidateDocxPath, nameof(candidateDocxPath));
        ValidatePath(outputDocxPath, nameof(outputDocxPath));

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(Path.GetDirectoryName(outputDocxPath)!);

            var document = new Document(candidateDocxPath);
            EnsureParagraphStyle(document, "正文");
            var stemParagraph = document
                .GetChildNodes(NodeType.Paragraph, true)
                .OfType<Paragraph>()
                .FirstOrDefault(IsEffectiveStemParagraph);

            if (stemParagraph is not null)
            {
                stemParagraph.ParagraphFormat.ClearFormatting();
                stemParagraph.ParagraphFormat.StyleName = "正文";
                foreach (Run run in stemParagraph.GetChildNodes(NodeType.Run, true).OfType<Run>())
                {
                    run.Font.ClearFormatting();
                }
            }

            document.Save(outputDocxPath);
        }, cancellationToken);
    }

    private bool IsQuestionStartParagraph(Paragraph paragraph)
    {
        return !string.IsNullOrWhiteSpace(paragraph.GetText())
            && (_importStyleOptions.IsQuestionStartStyle(paragraph.ParagraphFormat.StyleName)
                || _importStyleOptions.IsQuestionStartStyle(paragraph.ParagraphFormat.Style?.Name));
    }

    private static bool IsEffectiveStemParagraph(Paragraph paragraph)
    {
        var text = paragraph.GetText();
        if (string.IsNullOrWhiteSpace(text)
            || text.Contains("Created with an evaluation copy of Aspose.Words", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Evaluation Only. Created with Aspose.Words", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var styleNamePartType = QuestionPartStyleOptions.Default.ResolvePartType(paragraph.ParagraphFormat.StyleName);
        var stylePartType = QuestionPartStyleOptions.Default.ResolvePartType(paragraph.ParagraphFormat.Style?.Name);
        return styleNamePartType is ContentBlockPartType.Stem
            || stylePartType is ContentBlockPartType.Stem;
    }

    private static void SaveCandidateIfAny(
        Document sourceDocument,
        IReadOnlyList<Node> sourceNodes,
        string candidateDirectory,
        List<QuestionImportCandidateDocumentResult> results)
    {
        if (sourceNodes.Count == 0)
        {
            return;
        }

        var sortOrder = results.Count + 1;
        var candidateId = $"q{sortOrder:000}";
        var candidateDocxPath = Path.Combine(candidateDirectory, candidateId, "candidate.docx");
        var candidateHtmlPath = Path.Combine(candidateDirectory, candidateId, "preview.html");
        Directory.CreateDirectory(Path.GetDirectoryName(candidateDocxPath)!);

        var candidateDocument = new Document();
        var body = candidateDocument.FirstSection.Body;
        body.RemoveAllChildren();
        var importer = new NodeImporter(sourceDocument, candidateDocument, ImportFormatMode.KeepSourceFormatting);

        foreach (var sourceNode in sourceNodes)
        {
            body.AppendChild(importer.ImportNode(sourceNode, isImportChildren: true));
        }

        candidateDocument.Save(candidateDocxPath);
        results.Add(new QuestionImportCandidateDocumentResult(
            candidateId,
            sortOrder,
            candidateDocxPath,
            candidateHtmlPath));
    }

    private static void EnsureParagraphStyle(Document document, string styleName)
    {
        if (document.Styles[styleName] is not null)
        {
            return;
        }

        document.Styles.Add(StyleType.Paragraph, styleName);
    }

    private static void ValidatePath(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Path cannot be empty.", parameterName);
        }
    }
}
