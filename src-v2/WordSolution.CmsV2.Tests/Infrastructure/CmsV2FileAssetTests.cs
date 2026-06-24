using System.IO.Compression;
using System.Net;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using Aspose.Words;
using WordSolution.CmsV2.Infrastructure.Documents;

namespace WordSolution.CmsV2.Tests.Infrastructure;

public sealed class CmsV2FileAssetTests
{
    [Fact]
    public void FileAssetPathProvider_uses_v2_content_block_directories()
    {
        var rootDirectory = CreateTempRoot();
        var provider = new CmsV2FileAssetPathProvider();

        var docxPath = provider.GetContentBlockDocxPath(rootDirectory, contentBlockId: 42, versionNumber: 3);
        var htmlPath = provider.GetContentBlockHtmlPreviewPath(rootDirectory, contentBlockId: 42, versionNumber: 3);
        var textPath = provider.GetContentBlockPlainTextPath(rootDirectory, contentBlockId: 42, versionNumber: 3);

        Assert.Equal(
            Path.GetFullPath(Path.Combine(rootDirectory, "content-blocks", "source", "42", "v3.docx")),
            docxPath);
        Assert.Equal(
            Path.GetFullPath(Path.Combine(rootDirectory, "content-blocks", "html", "42", "v3.html")),
            htmlPath);
        Assert.Equal(
            Path.GetFullPath(Path.Combine(rootDirectory, "content-blocks", "text", "42", "v3.txt")),
            textPath);
        Assert.False(docxPath.StartsWith(Path.GetFullPath(Path.Combine(rootDirectory, "source")), StringComparison.OrdinalIgnoreCase));
        Assert.False(htmlPath.StartsWith(Path.GetFullPath(Path.Combine(rootDirectory, "html")), StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain("question-bank.db", docxPath, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void FileAssetPathProvider_rejects_invalid_ids(int contentBlockId, int versionNumber)
    {
        var provider = new CmsV2FileAssetPathProvider();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => provider.GetContentBlockDocxPath(CreateTempRoot(), contentBlockId, versionNumber));
    }

    [Fact]
    public async Task LocalContentBlockFileStore_saves_docx_and_reads_text_assets()
    {
        var rootDirectory = CreateTempRoot();
        var provider = new CmsV2FileAssetPathProvider();
        var fileStore = new LocalContentBlockFileStore();
        var docxPath = provider.GetContentBlockDocxPath(rootDirectory, contentBlockId: 5, versionNumber: 1);
        var htmlPath = provider.GetContentBlockHtmlPreviewPath(rootDirectory, contentBlockId: 5, versionNumber: 1);
        var textPath = provider.GetContentBlockPlainTextPath(rootDirectory, contentBlockId: 5, versionNumber: 1);
        var docxBytes = Encoding.UTF8.GetBytes("fake docx bytes for storage boundary");

        await using (var docxStream = new MemoryStream(docxBytes))
        {
            await fileStore.SaveContentBlockDocxAsync(docxPath, docxStream);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(htmlPath)!);
        await File.WriteAllTextAsync(htmlPath, "<html><body>preview</body></html>");
        await fileStore.SavePlainTextAsync(textPath, "plain text");

        Assert.True(await fileStore.ExistsAsync(docxPath));
        Assert.Equal(docxBytes, await fileStore.ReadContentBlockDocxAsync(docxPath));
        Assert.Equal("<html><body>preview</body></html>", await fileStore.ReadHtmlPreviewAsync(htmlPath));
        Assert.Equal("plain text", await fileStore.ReadPlainTextAsync(textPath));
    }

    [Fact]
    public async Task AsposeContentBlockDocumentProcessor_creates_preview_and_extracts_plain_text()
    {
        var rootDirectory = CreateTempRoot();
        var templateDocxPath = Path.Combine(rootDirectory, "template.docx");
        await CreateMinimalDocxAsync(templateDocxPath, "模板正文");
        var processor = new AsposeContentBlockDocumentProcessor(templateDocxPath);
        var blankDocxPath = Path.Combine(rootDirectory, "blank.docx");
        var sourceDocxPath = Path.Combine(rootDirectory, "source.docx");
        var htmlPath = Path.Combine(rootDirectory, "preview.html");

        await processor.CreateBlankDocxAsync(blankDocxPath);
        await CreateMinimalDocxAsync(sourceDocxPath, "动能定理");
        await processor.GenerateHtmlPreviewAsync(sourceDocxPath, htmlPath);
        var plainText = await processor.ExtractPlainTextAsync(sourceDocxPath);

        Assert.True(new FileInfo(blankDocxPath).Length > 0);
        Assert.True(new FileInfo(htmlPath).Length > 0);
        Assert.Contains("动能定理", await File.ReadAllTextAsync(htmlPath));
        Assert.Contains("动能定理", plainText);
    }

    [Fact]
    public async Task AsposeContentBlockDocumentProcessor_exports_numbered_list_labels_inside_body_fragment()
    {
        var rootDirectory = CreateTempRoot();
        var templateDocxPath = Path.Combine(rootDirectory, "template.docx");
        var sourceDocxPath = Path.Combine(rootDirectory, "numbered.docx");
        var htmlPath = Path.Combine(rootDirectory, "numbered.html");
        await CreateMinimalDocxAsync(templateDocxPath, "模板正文");
        CreateNumberedListDocx(sourceDocxPath);
        var processor = new AsposeContentBlockDocumentProcessor(templateDocxPath);

        await processor.GenerateHtmlPreviewAsync(sourceDocxPath, htmlPath);

        var bodyText = ExtractBodyText(await File.ReadAllTextAsync(htmlPath));
        Assert.Matches(@"1\.?\s*第一条", bodyText);
        Assert.Matches(@"2\.?\s*第二条", bodyText);
    }

    [Fact]
    public async Task AsposeContentBlockDocumentProcessor_copies_default_template_when_creating_initial_docx()
    {
        var rootDirectory = CreateTempRoot();
        var templateDocxPath = Path.Combine(rootDirectory, "template.docx");
        var initialDocxPath = Path.Combine(rootDirectory, "initial.docx");
        await CreateMinimalDocxAsync(templateDocxPath, "默认内容块模板");
        var processor = new AsposeContentBlockDocumentProcessor(templateDocxPath);

        await processor.CreateBlankDocxAsync(initialDocxPath);

        Assert.True(File.Exists(initialDocxPath));
        Assert.Contains("默认内容块模板", await processor.ExtractPlainTextAsync(initialDocxPath));
    }

    private static string CreateTempRoot()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "cms-v2-file-asset-tests",
            Guid.NewGuid().ToString("N"));
    }

    private static void CreateNumberedListDocx(string docxPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);

        var document = new Document();
        var builder = new DocumentBuilder(document);
        builder.ListFormat.ApplyNumberDefault();
        builder.Writeln("第一条");
        builder.Writeln("第二条");
        builder.ListFormat.RemoveNumbers();
        document.Save(docxPath);
    }

    private static string ExtractBodyText(string html)
    {
        var bodyMatch = Regex.Match(html, "<body[^>]*>([\\s\\S]*?)</body>", RegexOptions.IgnoreCase);
        var bodyHtml = bodyMatch.Success ? bodyMatch.Groups[1].Value : html;
        var withoutTags = Regex.Replace(bodyHtml, "<[^>]+>", string.Empty);
        return WebUtility.HtmlDecode(withoutTags).Replace("\u00a0", " ");
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
