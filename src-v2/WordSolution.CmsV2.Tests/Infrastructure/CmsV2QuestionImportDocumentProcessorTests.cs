using Aspose.Words;
using WordSolution.CmsV2.Infrastructure.Documents;

namespace WordSolution.CmsV2.Tests.Infrastructure;

public sealed class CmsV2QuestionImportDocumentProcessorTests
{
    [Fact]
    public async Task SplitCandidates_keeps_default_template_styles_in_candidate_docx()
    {
        var rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-question-import-processor-tests",
            Guid.NewGuid().ToString("N"));
        var templatePath = Path.Combine(rootDirectory, "template.docx");
        var sourcePath = Path.Combine(rootDirectory, "source.docx");
        var candidateDirectory = Path.Combine(rootDirectory, "candidates");
        try
        {
            CreateTemplateDocx(templatePath);
            await CreateTemplateBasedSourceDocxAsync(templatePath, sourcePath);
            var processor = new AsposeQuestionImportDocumentProcessor(templatePath);

            var candidates = await processor.SplitCandidatesAsync(sourcePath, candidateDirectory);

            var candidate = Assert.Single(candidates);
            var candidateDocument = new Document(candidate.DocxPath);
            Assert.NotNull(candidateDocument.Styles["答案"]);
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task CreateNeutralizedCandidateDocx_rebinds_first_stem_to_builtin_normal_and_keeps_template_styles()
    {
        var rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-question-import-processor-tests",
            Guid.NewGuid().ToString("N"));
        var templatePath = Path.Combine(rootDirectory, "template.docx");
        var sourcePath = Path.Combine(rootDirectory, "candidate.docx");
        var outputPath = Path.Combine(rootDirectory, "formal.docx");
        try
        {
            CreateTemplateDocx(templatePath);
            CreateCandidateWithOtherBeforeStem(sourcePath);
            var processor = new AsposeQuestionImportDocumentProcessor(templatePath);

            await processor.CreateNeutralizedCandidateDocxAsync(sourcePath, outputPath);

            var outputDocument = new Document(outputPath);
            AssertParagraphStyle(outputDocument, "Other lead", "未知样式");
            AssertParagraphStyleIdentifier(outputDocument, "Actual stem", StyleIdentifier.Normal);
            Assert.DoesNotContain(
                outputDocument.Styles.Cast<Style>(),
                style => style.Type == StyleType.Paragraph
                    && string.Equals(style.Name, "正文", StringComparison.Ordinal));
            Assert.NotNull(outputDocument.Styles["答案"]);
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    private static void CreateTemplateDocx(string docxPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);
        var document = new Document();
        EnsureParagraphStyle(document, "例题");
        EnsureParagraphStyle(document, "答案");
        EnsureParagraphStyle(document, "解析");
        document.Save(docxPath);
    }

    private static async Task CreateTemplateBasedSourceDocxAsync(string templatePath, string docxPath)
    {
        var processor = new AsposeContentBlockDocumentProcessor(templatePath);
        await processor.CreateBlankDocxAsync(docxPath);

        var document = new Document(docxPath);
        var body = document.FirstSection.Body;
        body.RemoveAllChildren();

        AddStyledParagraph(document, body, "例题", "Actual stem");
        AddPlainParagraph(document, body, "【答案】C");

        document.Save(docxPath);
    }

    private static void CreateCandidateWithOtherBeforeStem(string docxPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);
        var document = new Document();
        var body = document.FirstSection.Body;
        body.RemoveAllChildren();

        AddStyledParagraph(document, body, "未知样式", "Other lead");
        AddStyledParagraph(document, body, "例题", "Actual stem");

        document.Save(docxPath);
    }

    private static void AddStyledParagraph(Document document, Body body, string styleName, string text)
    {
        EnsureParagraphStyle(document, styleName);
        var paragraph = new Paragraph(document);
        paragraph.ParagraphFormat.StyleName = styleName;
        paragraph.AppendChild(new Run(document, text));
        body.AppendChild(paragraph);
    }

    private static void AddPlainParagraph(Document document, Body body, string text)
    {
        var paragraph = new Paragraph(document);
        paragraph.AppendChild(new Run(document, text));
        body.AppendChild(paragraph);
    }

    private static void EnsureParagraphStyle(Document document, string styleName)
    {
        if (document.Styles[styleName] is not null)
        {
            return;
        }

        document.Styles.Add(StyleType.Paragraph, styleName);
    }

    private static void AssertParagraphStyle(Document document, string text, string expectedStyleName)
    {
        var paragraph = document
            .GetChildNodes(NodeType.Paragraph, true)
            .OfType<Paragraph>()
            .Single(item => item.GetText().Contains(text, StringComparison.Ordinal));

        Assert.Equal(expectedStyleName, paragraph.ParagraphFormat.StyleName);
    }

    private static void AssertParagraphStyleIdentifier(
        Document document,
        string text,
        StyleIdentifier expectedStyleIdentifier)
    {
        var paragraph = document
            .GetChildNodes(NodeType.Paragraph, true)
            .OfType<Paragraph>()
            .Single(item => item.GetText().Contains(text, StringComparison.Ordinal));

        Assert.Equal(expectedStyleIdentifier, paragraph.ParagraphFormat.StyleIdentifier);
    }
}
