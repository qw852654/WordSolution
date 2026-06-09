using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Security;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WordSolution.CmsV2.Application.AtomicSections;
using WordSolution.CmsV2.Application.ContentBlocks;
using WordSolution.CmsV2.Application.Handouts;
using WordSolution.CmsV2.Application.SectionVariants;
using WordSolution.CmsV2.Application.Sections;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Repositories;
using WordSolution.CmsV2.Infrastructure.Persistence;

namespace WordSolution.CmsV2.Tests.Api;

public sealed class CmsV2ApiIntegrationTests
{
    [Fact]
    public async Task Api_startup_creates_v2_database_and_registers_core_services()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();

        var health = await client.GetFromJsonAsync<JsonElement>("/api/cms-v2/health");
        var enums = await client.GetFromJsonAsync<JsonElement>("/api/cms-v2/meta/enums");

        using var scope = factory.Services.CreateScope();
        Assert.Equal("ok", health.GetProperty("status").GetString());
        Assert.Equal(factory.BankRootDirectory, health.GetProperty("bankRootDirectory").GetString());
        Assert.True(File.Exists(Path.Combine(factory.BankRootDirectory, "cms-v2.db")));
        Assert.False(File.Exists(Path.Combine(factory.BankRootDirectory, "question-bank.db")));
        Assert.Contains("KnowledgePoint", enums.GetProperty("contentBlockType").EnumerateArray().Select(x => x.GetString()));
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<CmsV2DbContext>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ICmsV2UnitOfWork>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ICmsV2FileAssetPathProvider>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IContentBlockFileStore>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IContentBlockDocumentProcessor>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IHandoutDocumentGenerator>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ContentBlockUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ContentBlockDocumentUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ContentBlockRelationUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<SectionUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<AtomicSectionUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<SectionVariantUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<HandoutUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<HandoutGenerationUseCases>());
    }

    [Fact]
    public async Task ContentBlock_document_endpoints_create_import_preview_and_download_versions()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();

        var created = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks/blank-document",
            new
            {
                title = "动能定理",
                blockType = "KnowledgePoint",
                summary = "合外力做功",
                difficulty = "Medium",
                status = "Draft"
            });
        var contentBlockId = created.GetProperty("contentBlockId").GetInt32();
        var firstVersionId = created.GetProperty("contentBlockVersionId").GetInt32();

        var detail = await client.GetFromJsonAsync<JsonElement>($"/api/cms-v2/content-blocks/{contentBlockId}");
        var versions = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/content-blocks/{contentBlockId}/versions")
            ?? [];
        var docx = await client.GetAsync($"/api/cms-v2/content-blocks/{contentBlockId}/docx");
        var html = await client.GetStringAsync($"/api/cms-v2/content-blocks/{contentBlockId}/html-preview");
        var importDocxPath = Path.Combine(factory.BankRootDirectory, "imports", "source.docx");
        await CreateMinimalDocxAsync(importDocxPath, "导入后的正文");
        await using var importStream = File.OpenRead(importDocxPath);
        using var form = new MultipartFormDataContent
        {
            { new StreamContent(importStream), "file", "source.docx" },
            { new StringContent("true"), "setAsCurrent" }
        };
        var importedResponse = await client.PostAsync($"/api/cms-v2/content-blocks/{contentBlockId}/versions/import", form);
        var imported = await ReadSuccessJsonAsync(importedResponse);
        var updatedVersions = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/content-blocks/{contentBlockId}/versions")
            ?? [];
        var setCurrent = await PostJsonAsync(
            client,
            $"/api/cms-v2/content-blocks/{contentBlockId}/current-version",
            new { contentBlockVersionId = firstVersionId });

        Assert.Equal("动能定理", detail.GetProperty("title").GetString());
        Assert.Single(versions);
        Assert.Equal(firstVersionId, versions[0].GetProperty("id").GetInt32());
        Assert.Equal(HttpStatusCode.OK, docx.StatusCode);
        Assert.NotEmpty(await docx.Content.ReadAsByteArrayAsync());
        Assert.Contains("<html", html, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, imported.GetProperty("versionNumber").GetInt32());
        Assert.Equal(2, updatedVersions.Length);
        Assert.Equal(firstVersionId, setCurrent.GetProperty("contentBlockVersionId").GetInt32());
    }

    [Fact]
    public async Task Composition_and_handout_endpoints_generate_word_and_expose_generated_files()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var templatePath = Path.Combine(factory.BankRootDirectory, "templates", "default.docx");
        await CreateMinimalDocxAsync(templatePath, "模板正文");
        var importedBlock = await CreateImportedContentBlockAsync(client, factory.BankRootDirectory, "机械能守恒", "讲义正文");
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "机械能", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "机械能专题", type = "NormalCourse", difficulty = "Medium", status = "Draft" });
        var sectionItem = await PostJsonAsync(
            client,
            $"/api/cms-v2/sections/{section.GetProperty("id").GetInt32()}/items",
            new
            {
                targetType = "ContentBlock",
                targetId = importedBlock.ContentBlockId,
                referenceMode = "LockedVersion",
                lockedContentBlockVersionId = importedBlock.ImportedVersionId,
                sortOrder = 1
            });
        var variant = await PostJsonAsync(
            client,
            "/api/cms-v2/section-variants",
            new { sectionId = section.GetProperty("id").GetInt32(), title = "课堂版", type = "Lecture", difficulty = "Medium", status = "Draft" });
        await PostJsonAsync(
            client,
            $"/api/cms-v2/section-variants/{variant.GetProperty("id").GetInt32()}/items",
            new { sectionItemId = sectionItem.GetProperty("id").GetInt32(), sortOrder = 1 });
        var atomic = await PostJsonAsync(client, "/api/cms-v2/atomic-sections", new { title = "原子片段", type = "Custom", status = "Draft" });
        await PostJsonAsync(
            client,
            $"/api/cms-v2/atomic-sections/{atomic.GetProperty("id").GetInt32()}/items",
            new { contentBlockId = importedBlock.ContentBlockId, referenceMode = "FollowLatest", sortOrder = 1 });
        var handout = await PostJsonAsync(client, "/api/cms-v2/handouts", new { title = "机械能讲义", status = "Draft" });
        var handoutVersion = await PostJsonAsync(
            client,
            $"/api/cms-v2/handouts/{handout.GetProperty("id").GetInt32()}/versions",
            new { title = "基础班", type = "Normal", status = "Draft", sortOrder = 1 });
        await PostJsonAsync(
            client,
            $"/api/cms-v2/handout-versions/{handoutVersion.GetProperty("id").GetInt32()}/items",
            new { targetType = "SectionVariant", targetId = variant.GetProperty("id").GetInt32(), sortOrder = 1 });
        var template = await PostJsonAsync(
            client,
            "/api/cms-v2/output-templates",
            new { title = "默认模板", templateDocxPath = templatePath, status = "Active" });
        var outputForm = await PostJsonAsync(
            client,
            "/api/cms-v2/output-forms",
            new
            {
                handoutVersionId = handoutVersion.GetProperty("id").GetInt32(),
                outputTemplateId = template.GetProperty("id").GetInt32(),
                title = "学生版",
                audience = "Student",
                outputFormat = "Word",
                visibilityMode = "StudentNoAnswer",
                status = "Active"
            });

        var generated = await PostJsonAsync(
            client,
            $"/api/cms-v2/output-forms/{outputForm.GetProperty("id").GetInt32()}/generate-word",
            new { generatedTime = "2026-06-09T00:00:00Z" });
        var generatedFileId = generated.GetProperty("generatedFileId").GetInt32();
        var generatedFiles = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/output-forms/{outputForm.GetProperty("id").GetInt32()}/generated-files")
            ?? [];
        var manifest = await client.GetFromJsonAsync<JsonElement>($"/api/cms-v2/generated-files/{generatedFileId}/manifest");
        var download = await client.GetAsync($"/api/cms-v2/generated-files/{generatedFileId}/download");

        Assert.True(File.Exists(generated.GetProperty("filePath").GetString()));
        Assert.Single(generatedFiles);
        Assert.Equal(generatedFileId, generatedFiles[0].GetProperty("id").GetInt32());
        Assert.Equal(1, manifest.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(importedBlock.ImportedVersionId, manifest.GetProperty("sources")[0].GetProperty("contentBlockVersionId").GetInt32());
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.NotEmpty(await download.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Api_returns_problem_details_for_validation_and_not_found_failures()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();

        var invalidBlock = await client.PostAsJsonAsync(
            "/api/cms-v2/content-blocks/blank-document",
            new { title = " ", blockType = "KnowledgePoint" });
        var missingBlock = await client.GetAsync("/api/cms-v2/content-blocks/404");
        var missingParent = await client.PostAsJsonAsync(
            "/api/cms-v2/sections",
            new { teachingTopicId = 404, title = "孤立小节" });

        Assert.Equal(HttpStatusCode.BadRequest, invalidBlock.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingBlock.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingParent.StatusCode);
    }

    private static async Task<ImportedContentBlock> CreateImportedContentBlockAsync(
        HttpClient client,
        string bankRootDirectory,
        string title,
        string text)
    {
        var created = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks/blank-document",
            new { title, blockType = "KnowledgePoint", status = "Draft" });
        var contentBlockId = created.GetProperty("contentBlockId").GetInt32();
        var importDocxPath = Path.Combine(bankRootDirectory, "imports", $"{Guid.NewGuid():N}.docx");
        await CreateMinimalDocxAsync(importDocxPath, text);
        await using var importStream = File.OpenRead(importDocxPath);
        using var form = new MultipartFormDataContent
        {
            { new StreamContent(importStream), "file", "source.docx" },
            { new StringContent("true"), "setAsCurrent" }
        };
        var importedResponse = await client.PostAsync($"/api/cms-v2/content-blocks/{contentBlockId}/versions/import", form);
        var imported = await ReadSuccessJsonAsync(importedResponse);

        return new ImportedContentBlock(contentBlockId, imported.GetProperty("contentBlockVersionId").GetInt32());
    }

    private static async Task<JsonElement> PostJsonAsync(HttpClient client, string uri, object value)
    {
        var response = await client.PostAsJsonAsync(uri, value);
        return await ReadSuccessJsonAsync(response);
    }

    private static async Task<JsonElement> ReadSuccessJsonAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);
        return JsonDocument.Parse(body).RootElement.Clone();
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

    private sealed record ImportedContentBlock(int ContentBlockId, int ImportedVersionId);

    private sealed class CmsV2ApiFactory : WebApplicationFactory<Program>
    {
        public string BankRootDirectory { get; } = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-api-tests",
            Guid.NewGuid().ToString("N"));

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["CmsV2:BankRootDirectory"] = BankRootDirectory
                });
            });
        }
    }
}
