using Aspose.Words;
using WordSolution.CmsV2.Infrastructure.Documents;

namespace WordSolution.CmsV2.Tests.Infrastructure;

public sealed class CmsV2QuestionImportDocumentProcessorTests
{
    [Fact]
    public async Task CreateNeutralizedCandidateDocx_rebinds_first_stem_only_and_keeps_other_style()
    {
        var rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-question-import-processor-tests",
            Guid.NewGuid().ToString("N"));
        var sourcePath = Path.Combine(rootDirectory, "candidate.docx");
        var outputPath = Path.Combine(rootDirectory, "formal.docx");
        try
        {
            CreateCandidateWithOtherBeforeStem(sourcePath);
            var processor = new AsposeQuestionImportDocumentProcessor();

            await processor.CreateNeutralizedCandidateDocxAsync(sourcePath, outputPath);

            var outputDocument = new Document(outputPath);
            AssertParagraphStyle(outputDocument, "Other lead", "未知样式");
            AssertParagraphStyle(outputDocument, "Actual stem", "正文");
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
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
}
