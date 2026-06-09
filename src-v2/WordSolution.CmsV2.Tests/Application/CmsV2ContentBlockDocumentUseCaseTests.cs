using System.IO.Compression;
using System.Security;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Application.ContentBlocks;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Infrastructure.Documents;
using WordSolution.CmsV2.Infrastructure.Persistence;
using WordSolution.CmsV2.Infrastructure.Repositories;

namespace WordSolution.CmsV2.Tests.Application;

public sealed class CmsV2ContentBlockDocumentUseCaseTests
{
    [Fact]
    public async Task CreateContentBlockWithBlankDocument_creates_metadata_and_file_assets()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var useCases = CreateUseCases(unitOfWork);

        var result = await useCases.CreateContentBlockWithBlankDocumentAsync(
            new CreateContentBlockWithBlankDocumentCommand(
                bankRootDirectory,
                "机械能守恒",
                ContentBlockType.KnowledgePoint,
                Summary: "守恒条件"));

        var block = await unitOfWork.ContentBlocks.GetByIdAsync(result.ContentBlockId);
        var version = await unitOfWork.ContentBlockVersions.GetByIdAsync(result.ContentBlockVersionId);

        Assert.NotNull(block);
        Assert.NotNull(version);
        Assert.Equal(result.ContentBlockVersionId, block.CurrentVersionId);
        Assert.Equal(1, result.VersionNumber);
        Assert.True(File.Exists(result.DocxPath));
        Assert.True(File.Exists(result.HtmlPreviewPath));
        Assert.True(File.Exists(result.PlainTextPath));
        Assert.Equal(result.DocxPath, version.DocxPath);
        Assert.Equal(result.HtmlPreviewPath, version.HtmlPreviewPath);
        Assert.True(version.IsCurrent);
    }

    [Fact]
    public async Task ImportContentBlockDocxVersion_increments_version_and_switches_current_version()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var useCases = CreateUseCases(unitOfWork);
        var created = await useCases.CreateContentBlockWithBlankDocumentAsync(
            new CreateContentBlockWithBlankDocumentCommand(
                bankRootDirectory,
                "动能定理",
                ContentBlockType.KnowledgePoint));
        var importDocxPath = Path.Combine(bankRootDirectory, "imports", "source.docx");
        await CreateMinimalDocxAsync(importDocxPath, "合外力做功等于动能变化");

        await using var importStream = File.OpenRead(importDocxPath);
        var imported = await useCases.ImportContentBlockDocxVersionAsync(
            new ImportContentBlockDocxVersionCommand(
                bankRootDirectory,
                created.ContentBlockId,
                importStream,
                SetAsCurrent: true));

        var block = await unitOfWork.ContentBlocks.GetByIdAsync(created.ContentBlockId);
        var versions = await unitOfWork.ContentBlockVersions.ListByContentBlockAsync(created.ContentBlockId);
        var textFile = await File.ReadAllTextAsync(imported.PlainTextPath);

        Assert.NotNull(block);
        Assert.Equal(imported.ContentBlockVersionId, block.CurrentVersionId);
        Assert.Equal(2, imported.VersionNumber);
        Assert.Equal([1, 2], versions.Select(version => version.VersionNumber));
        Assert.False(versions.Single(version => version.VersionNumber == 1).IsCurrent);
        Assert.True(versions.Single(version => version.VersionNumber == 2).IsCurrent);
        Assert.Contains("合外力做功等于动能变化", versions.Single(version => version.VersionNumber == 2).PlainText);
        Assert.Contains("合外力做功等于动能变化", textFile);
        Assert.True(File.Exists(imported.DocxPath));
        Assert.True(File.Exists(imported.HtmlPreviewPath));
    }

    [Fact]
    public async Task ContentBlockDocumentUseCases_reject_invalid_input_without_creating_assets()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var useCases = CreateUseCases(unitOfWork);
        var bankRootDirectory = CreateTempRoot();

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => useCases.CreateContentBlockWithBlankDocumentAsync(
                new CreateContentBlockWithBlankDocumentCommand(
                    " ",
                    "无效路径",
                    ContentBlockType.GeneralText)));

        await using var emptyStream = new MemoryStream();
        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => useCases.ImportContentBlockDocxVersionAsync(
                new ImportContentBlockDocxVersionCommand(
                    bankRootDirectory,
                    ContentBlockId: 404,
                    emptyStream)));

        Assert.False(Directory.Exists(Path.Combine(bankRootDirectory, "content-blocks")));
        Assert.Empty(await unitOfWork.ContentBlocks.ListAsync());
        Assert.Empty(await unitOfWork.ContentBlockVersions.ListAsync());
    }

    [Fact]
    public async Task ImportContentBlockDocxVersion_cleans_up_file_assets_when_processing_fails()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var created = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
            new CreateContentBlockWithInitialVersionCommand(
                "已有内容块",
                ContentBlockType.GeneralText,
                "existing/v1.docx"));
        var pathProvider = new CmsV2FileAssetPathProvider();
        var fileStore = new LocalContentBlockFileStore();
        var useCases = new ContentBlockDocumentUseCases(
            unitOfWork,
            pathProvider,
            fileStore,
            new FailingContentBlockDocumentProcessor());
        var expectedDocxPath = pathProvider.GetContentBlockDocxPath(bankRootDirectory, created.Id, versionNumber: 2);

        await using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("not a real docx, processor will fail before reading it"));
        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => useCases.ImportContentBlockDocxVersionAsync(
                new ImportContentBlockDocxVersionCommand(
                    bankRootDirectory,
                    created.Id,
                    inputStream)));

        Assert.False(File.Exists(expectedDocxPath));
        Assert.Single(await unitOfWork.ContentBlockVersions.ListByContentBlockAsync(created.Id));
    }

    private static ContentBlockDocumentUseCases CreateUseCases(EfCmsV2UnitOfWork unitOfWork)
    {
        return new ContentBlockDocumentUseCases(
            unitOfWork,
            new CmsV2FileAssetPathProvider(),
            new LocalContentBlockFileStore(),
            new AsposeContentBlockDocumentProcessor());
    }

    private static async Task<CmsV2DbContext> CreateMigratedContextAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-document-use-case-tests",
            Guid.NewGuid().ToString("N"),
            "cms-v2.db");

        var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        return context;
    }

    private static string CreateTempRoot()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "cms-v2-document-use-case-tests",
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

    private sealed class FailingContentBlockDocumentProcessor : IContentBlockDocumentProcessor
    {
        public Task CreateBlankDocxAsync(string docxPath, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("The test processor is configured to fail.");
        }

        public Task GenerateHtmlPreviewAsync(
            string docxPath,
            string htmlPreviewPath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("The test processor is configured to fail.");
        }

        public Task<string> ExtractPlainTextAsync(string docxPath, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("The test processor is configured to fail.");
        }
    }
}
