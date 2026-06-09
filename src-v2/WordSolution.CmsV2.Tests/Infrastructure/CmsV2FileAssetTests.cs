using System.IO.Compression;
using System.Security;
using System.Text;
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
        var processor = new AsposeContentBlockDocumentProcessor();
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

    private static string CreateTempRoot()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "cms-v2-file-asset-tests",
            Guid.NewGuid().ToString("N"));
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
