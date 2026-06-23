using System.IO.Compression;
using System.Security;
using System.Text;
using Aspose.Words;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Infrastructure.Documents;
using AsposeDocument = Aspose.Words.Document;
using AsposeParagraph = Aspose.Words.Paragraph;
using AsposeSaveFormat = Aspose.Words.SaveFormat;

namespace WordSolution.CmsV2.Tests.Infrastructure;

public sealed class CmsV2HandoutDocumentGenerationTests
{
    [Fact]
    public void FileAssetPathProvider_uses_v2_generated_handout_directory_and_sanitizes_title()
    {
        var rootDirectory = CreateTempRoot();
        var generatedTime = new DateTimeOffset(2026, 6, 9, 8, 7, 6, 5, TimeSpan.Zero);
        var provider = new CmsV2FileAssetPathProvider();

        var path = provider.GetGeneratedHandoutDocxPath(
            rootDirectory,
            handoutVersionId: 12,
            outputFormId: 34,
            outputFormTitle: "学生版:提升/训练*?",
            generatedTime);

        Assert.StartsWith(
            Path.GetFullPath(Path.Combine(rootDirectory, "handouts", "generated", "12")),
            path,
            StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith(".docx", path);
        Assert.Contains("20260609080706005-34-", Path.GetFileName(path));
        Assert.DoesNotContain(":", Path.GetFileName(path));
        Assert.DoesNotContain("/", Path.GetFileName(path));
        Assert.DoesNotContain("*", Path.GetFileName(path));
        Assert.DoesNotContain("?", Path.GetFileName(path));
    }

    [Fact]
    public async Task AsposeHandoutDocumentGenerator_combines_template_and_sources_in_order()
    {
        var rootDirectory = CreateTempRoot();
        var templatePath = Path.Combine(rootDirectory, "templates", "default.docx");
        var firstSourcePath = Path.Combine(rootDirectory, "content-blocks", "source", "1", "v1.docx");
        var secondSourcePath = Path.Combine(rootDirectory, "content-blocks", "source", "2", "v1.docx");
        var outputPath = Path.Combine(rootDirectory, "handouts", "generated", "1", "output.docx");
        var generator = new AsposeHandoutDocumentGenerator();

        await CreateMinimalDocxAsync(templatePath, "模板页眉区域");
        await CreateMinimalDocxAsync(firstSourcePath, "第一块正文");
        await CreateMinimalDocxAsync(secondSourcePath, "第二块正文");

        await generator.GenerateWordAsync(
            "机械能守恒讲义",
            templatePath,
            [
                new HandoutDocumentSource("知识点", firstSourcePath),
                new HandoutDocumentSource("例题", secondSourcePath)
            ],
            outputPath,
            new DateTimeOffset(2026, 6, 9, 8, 0, 0, TimeSpan.Zero));

        var text = ReadDocxText(outputPath);

        Assert.Contains("机械能守恒讲义", text);
        Assert.Contains("模板页眉区域", text);
        Assert.True(text.IndexOf("第一块正文", StringComparison.Ordinal) < text.IndexOf("第二块正文", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AsposeHandoutDocumentGenerator_preserves_output_template_headers_and_footers()
    {
        var rootDirectory = CreateTempRoot();
        var templatePath = Path.Combine(rootDirectory, "templates", "header-footer-template.docx");
        var sourcePath = Path.Combine(rootDirectory, "content-blocks", "source", "1", "v1.docx");
        var outputPath = Path.Combine(rootDirectory, "handouts", "generated", "1", "output.docx");
        var generator = new AsposeHandoutDocumentGenerator();

        CreateTemplateWithHeaderAndFooter(templatePath, "模板页眉", "模板页脚", "模板正文");
        await CreateMinimalDocxAsync(sourcePath, "内容正文");

        await generator.GenerateWordAsync(
            "讲义",
            templatePath,
            [new HandoutDocumentSource("内容块", sourcePath)],
            outputPath,
            new DateTimeOffset(2026, 6, 23, 8, 0, 0, TimeSpan.Zero));

        var outputDocument = new AsposeDocument(outputPath);
        var firstSection = outputDocument.FirstSection;

        Assert.NotNull(firstSection.HeadersFooters[HeaderFooterType.HeaderPrimary]);
        Assert.NotNull(firstSection.HeadersFooters[HeaderFooterType.FooterPrimary]);
        Assert.Contains("模板页眉", firstSection.HeadersFooters[HeaderFooterType.HeaderPrimary]!.GetText());
        Assert.Contains("模板页脚", firstSection.HeadersFooters[HeaderFooterType.FooterPrimary]!.GetText());
    }

    [Fact]
    public async Task AsposeHandoutDocumentGenerator_rebases_example_numbering_across_source_documents()
    {
        var rootDirectory = CreateTempRoot();
        var templatePath = Path.Combine(rootDirectory, "templates", "numbering-template.docx");
        var firstSourcePath = Path.Combine(rootDirectory, "content-blocks", "source", "1", "v1.docx");
        var secondSourcePath = Path.Combine(rootDirectory, "content-blocks", "source", "2", "v1.docx");
        var outputPath = Path.Combine(rootDirectory, "handouts", "generated", "1", "output.docx");
        var generator = new AsposeHandoutDocumentGenerator();

        CreateTemplateWithExampleNumberingStyle(templatePath);
        CreateExampleNumberedDocx(firstSourcePath, "第一道例题");
        CreateExampleNumberedDocx(secondSourcePath, "第二道例题");

        await generator.GenerateWordAsync(
            "讲义",
            templatePath,
            [
                new HandoutDocumentSource("例题一", firstSourcePath),
                new HandoutDocumentSource("例题二", secondSourcePath)
            ],
            outputPath,
            new DateTimeOffset(2026, 6, 23, 8, 0, 0, TimeSpan.Zero));

        var outputDocument = new AsposeDocument(outputPath);
        outputDocument.UpdateListLabels();
        var exampleLabels = outputDocument
            .GetChildNodes(NodeType.Paragraph, true)
            .OfType<AsposeParagraph>()
            .Where(paragraph => paragraph.GetText().Contains("道例题", StringComparison.Ordinal))
            .Select(paragraph => paragraph.ListLabel.LabelString)
            .ToArray();

        Assert.Equal(["1.", "2."], exampleLabels);
    }

    private static string CreateTempRoot()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "cms-v2-handout-generation-tests",
            Guid.NewGuid().ToString("N"));
    }

    private static string ReadDocxText(string docxPath)
    {
        var document = new AsposeDocument(docxPath);
        return document.ToString(AsposeSaveFormat.Text);
    }

    private static void CreateTemplateWithHeaderAndFooter(
        string docxPath,
        string headerText,
        string footerText,
        string bodyText)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);

        var document = new AsposeDocument();
        var builder = new DocumentBuilder(document);
        builder.MoveToHeaderFooter(HeaderFooterType.HeaderPrimary);
        builder.Write(headerText);
        builder.MoveToHeaderFooter(HeaderFooterType.FooterPrimary);
        builder.Write(footerText);
        builder.MoveToDocumentEnd();
        builder.Writeln(bodyText);
        document.Save(docxPath);
    }

    private static void CreateTemplateWithExampleNumberingStyle(string docxPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);

        var document = new AsposeDocument();
        var builder = new DocumentBuilder(document);
        builder.Writeln("模板正文");
        builder.ListFormat.ApplyNumberDefault();
        builder.ListFormat.ListLevel.NumberFormat = "\x0000.";
        builder.ListFormat.ListLevel.NumberStyle = NumberStyle.Arabic;
        builder.ListFormat.RemoveNumbers();
        document.Save(docxPath);
    }

    private static void CreateExampleNumberedDocx(string docxPath, string exampleText)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);

        var document = new AsposeDocument();
        var builder = new DocumentBuilder(document);
        builder.ListFormat.ApplyNumberDefault();
        builder.Writeln(exampleText);
        builder.ListFormat.RemoveNumbers();
        document.Save(docxPath);
    }

    private static async Task CreateMinimalDocxAsync(string docxPath, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);

        using var archive = ZipFile.Open(docxPath, ZipArchiveMode.Create);
        await WriteEntryAsync(
            archive,
            "[Content_Types].xml",
            """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
              <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
              <Default Extension="xml" ContentType="application/xml"/>
              <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
            </Types>
            """);
        await WriteEntryAsync(
            archive,
            "_rels/.rels",
            """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
            </Relationships>
            """);
        await WriteEntryAsync(
            archive,
            "word/document.xml",
            $$"""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
              <w:body>
                <w:p>
                  <w:r>
                    <w:t>{{SecurityElement.Escape(text)}}</w:t>
                  </w:r>
                </w:p>
                <w:sectPr/>
              </w:body>
            </w:document>
            """);
    }

    private static async Task WriteEntryAsync(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName);
        await using var stream = entry.Open();
        await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        await writer.WriteAsync(content);
    }
}
