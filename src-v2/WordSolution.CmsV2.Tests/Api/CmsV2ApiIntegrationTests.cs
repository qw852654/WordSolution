using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Security;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WordSolution.CmsV2.Application.AtomicSections;
using WordSolution.CmsV2.Application.ContentBlocks;
using WordSolution.CmsV2.Application.Handouts;
using WordSolution.CmsV2.Application.SectionVariants;
using WordSolution.CmsV2.Application.Sections;
using WordSolution.CmsV2.Application.TeachingStructure;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Enums;
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
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IContentBlockEditSessionStore>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IContentBlockEditSessionFileStore>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IContentBlockEditSessionLauncher>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IHandoutDocumentGenerator>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ContentBlockUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ContentBlockDocumentUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ContentBlockEditSessionUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ContentBlockRelationUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<SectionUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<AtomicSectionUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<SectionVariantUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<TeachingStructureUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<HandoutUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<HandoutGenerationUseCases>());
        Assert.Contains(
            factory.Services.GetServices<IHostedService>(),
            service => service.GetType().Name == "ContentBlockEditSessionBackgroundService");
    }

    [Fact]
    public async Task ContentBlock_document_endpoints_create_import_preview_and_download_versions()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "机械能", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "机械能专题", type = "NormalCourse", difficulty = "Medium", status = "Draft" });
        var sectionId = section.GetProperty("id").GetInt32();

        var created = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks/blank-document",
            new
            {
                sectionId,
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
    public async Task ContentBlock_edit_session_endpoints_create_sync_cancel_and_return_not_found()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "Mechanics", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new
            {
                teachingTopicId = topic.GetProperty("id").GetInt32(),
                title = "Energy",
                type = "NormalCourse",
                difficulty = "Medium",
                status = "Draft"
            });
        var createdBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks/blank-document",
            new
            {
                sectionId = section.GetProperty("id").GetInt32(),
                title = "Work energy theorem",
                blockType = "KnowledgePoint",
                difficulty = "Basic",
                status = "Draft"
            });
        var contentBlockId = createdBlock.GetProperty("contentBlockId").GetInt32();

        var firstSession = await PostJsonAsync(
            client,
            $"/api/cms-v2/content-blocks/{contentBlockId}/edit-session",
            new { openWord = false });
        var firstSessionId = firstSession.GetProperty("sessionId").GetString()!;
        var loadedSession = await client.GetFromJsonAsync<JsonElement>(
            $"/api/cms-v2/content-block-edit-sessions/{firstSessionId}");
        var unchangedSync = await PostJsonAsync(
            client,
            $"/api/cms-v2/content-block-edit-sessions/{firstSessionId}/sync",
            new { });

        var secondSession = await PostJsonAsync(
            client,
            $"/api/cms-v2/content-blocks/{contentBlockId}/edit-session",
            new { openWord = false });
        var secondSessionId = secondSession.GetProperty("sessionId").GetString()!;
        var editableDocxPath = Path.Combine(
            factory.BankRootDirectory,
            "edit-sessions",
            "content-blocks",
            secondSessionId,
            "edit.docx");
        File.Delete(editableDocxPath);
        await CreateMinimalDocxAsync(editableDocxPath, "Changed content");
        var changedSync = await PostJsonAsync(
            client,
            $"/api/cms-v2/content-block-edit-sessions/{secondSessionId}/sync",
            new { });

        var thirdSession = await PostJsonAsync(
            client,
            $"/api/cms-v2/content-blocks/{contentBlockId}/edit-session",
            new { openWord = false });
        var thirdSessionId = thirdSession.GetProperty("sessionId").GetString()!;
        var cancelled = await PostJsonAsync(
            client,
            $"/api/cms-v2/content-block-edit-sessions/{thirdSessionId}/cancel",
            new { });
        var missingSession = await client.GetAsync("/api/cms-v2/content-block-edit-sessions/missing-session");

        Assert.Equal(contentBlockId, firstSession.GetProperty("contentBlockId").GetInt32());
        Assert.Equal("None", firstSession.GetProperty("launchMode").GetString());
        Assert.False(firstSession.GetProperty("openedByServer").GetBoolean());
        Assert.Equal(firstSessionId, loadedSession.GetProperty("sessionId").GetString());
        Assert.False(unchangedSync.GetProperty("changed").GetBoolean());
        Assert.Equal("Synced", unchangedSync.GetProperty("status").GetString());
        Assert.True(changedSync.GetProperty("changed").GetBoolean());
        Assert.Equal(2, changedSync.GetProperty("currentVersionNumber").GetInt32());
        Assert.Equal("Cancelled", cancelled.GetProperty("status").GetString());
        Assert.Equal(HttpStatusCode.NotFound, missingSession.StatusCode);
    }

    [Fact]
    public async Task Insert_create_endpoints_persist_section_ownership_and_atomic_section_difficulty()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "功能关系", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new
            {
                teachingTopicId = topic.GetProperty("id").GetInt32(),
                title = "机械能守恒",
                type = "NormalCourse",
                difficulty = "Medium",
                status = "Draft"
            });
        var sectionId = section.GetProperty("id").GetInt32();

        var createdBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks/blank-document",
            new
            {
                sectionId,
                title = "守恒条件",
                blockType = "KnowledgePoint",
                difficulty = "Basic",
                status = "Draft"
            });
        var blockDetail = await client.GetFromJsonAsync<JsonElement>(
            $"/api/cms-v2/content-blocks/{createdBlock.GetProperty("contentBlockId").GetInt32()}");

        var createdAtomicSection = await PostJsonAsync(
            client,
            "/api/cms-v2/atomic-sections",
            new
            {
                sectionId,
                title = "基础讲解片段",
                description = "用于串联概念和例题",
                type = "Custom",
                difficulty = "Advanced",
                status = "Draft"
            });

        Assert.Equal(sectionId, blockDetail.GetProperty("sectionId").GetInt32());
        Assert.Equal(sectionId, createdAtomicSection.GetProperty("sectionId").GetInt32());
        Assert.Equal("Advanced", createdAtomicSection.GetProperty("difficulty").GetString());
    }

    [Fact]
    public async Task Content_block_can_be_created_without_title_but_atomic_section_still_requires_title()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "功能关系", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new
            {
                teachingTopicId = topic.GetProperty("id").GetInt32(),
                title = "机械能守恒",
                type = "NormalCourse",
                difficulty = "Medium",
                status = "Draft"
            });
        var sectionId = section.GetProperty("id").GetInt32();

        var createdBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks/blank-document",
            new
            {
                sectionId,
                title = string.Empty,
                blockType = "Question",
                difficulty = "Basic",
                status = "Draft"
            });
        var blockDetail = await client.GetFromJsonAsync<JsonElement>(
            $"/api/cms-v2/content-blocks/{createdBlock.GetProperty("contentBlockId").GetInt32()}");

        var invalidAtomicSection = await client.PostAsJsonAsync(
            "/api/cms-v2/atomic-sections",
            new
            {
                sectionId,
                title = string.Empty,
                type = "Custom",
                difficulty = "Basic",
                status = "Draft"
            });

        Assert.Equal(string.Empty, blockDetail.GetProperty("title").GetString());
        Assert.Equal(HttpStatusCode.BadRequest, invalidAtomicSection.StatusCode);
    }

    [Fact]
    public async Task Teaching_structure_endpoints_manage_topics_section_binding_and_read_tree()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var root = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "Energy", sortOrder = 10 });
        var rootId = root.GetProperty("id").GetInt32();
        var child = await PostJsonAsync(
            client,
            $"/api/cms-v2/teaching-topics/{rootId}/children",
            new { name = "Work energy theorem" });
        var sibling = await PostJsonAsync(
            client,
            $"/api/cms-v2/teaching-topics/{child.GetProperty("id").GetInt32()}/next-sibling",
            new { name = "Mechanical energy conservation" });
        var renamed = await PostJsonAsync(
            client,
            $"/api/cms-v2/teaching-topics/{sibling.GetProperty("id").GetInt32()}/rename",
            new { name = "Mechanical energy", description = "renamed" });
        var section = await PostJsonAsync(
            client,
            $"/api/cms-v2/teaching-topics/{sibling.GetProperty("id").GetInt32()}/section",
            new { title = "Mechanical energy section", difficulty = "Medium", status = "Draft" });
        var variant = await PostJsonAsync(
            client,
            "/api/cms-v2/section-variants",
            new { sectionId = section.GetProperty("id").GetInt32(), title = "Basic lecture", type = "Lecture", difficulty = "Basic", status = "Draft" });

        var duplicateSection = await client.PostAsJsonAsync(
            $"/api/cms-v2/teaching-topics/{sibling.GetProperty("id").GetInt32()}/section",
            new { title = "Duplicate" });
        var deleteNonEmpty = await client.DeleteAsync($"/api/cms-v2/teaching-topics/{rootId}");
        var deleteEmpty = await client.DeleteAsync($"/api/cms-v2/teaching-topics/{child.GetProperty("id").GetInt32()}");
        var tree = await client.GetFromJsonAsync<JsonElement[]>("/api/cms-v2/teaching-structure") ?? [];

        Assert.Equal("Mechanical energy", renamed.GetProperty("name").GetString());
        Assert.Equal("renamed", renamed.GetProperty("description").GetString());
        Assert.Equal("Mechanical energy section", section.GetProperty("title").GetString());
        Assert.Equal(HttpStatusCode.BadRequest, duplicateSection.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, deleteNonEmpty.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deleteEmpty.StatusCode);
        Assert.Single(tree);
        var rootNode = tree[0];
        Assert.Equal(rootId, rootNode.GetProperty("teachingTopic").GetProperty("id").GetInt32());
        Assert.True(rootNode.GetProperty("canSetDisplayRoot").GetBoolean());
        var children = rootNode.GetProperty("children").EnumerateArray().ToArray();
        Assert.Single(children);
        Assert.Equal(sibling.GetProperty("id").GetInt32(), children[0].GetProperty("teachingTopic").GetProperty("id").GetInt32());
        Assert.Equal(section.GetProperty("id").GetInt32(), children[0].GetProperty("section").GetProperty("id").GetInt32());
        Assert.Equal(variant.GetProperty("id").GetInt32(), children[0].GetProperty("sectionVariants")[0].GetProperty("id").GetInt32());
        Assert.False(children[0].GetProperty("canDelete").GetBoolean());
    }

    [Fact]
    public async Task Atomic_section_section_item_operations_rename_move_remove_and_insert_child_content_block()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "Mechanics", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new
            {
                teachingTopicId = topic.GetProperty("id").GetInt32(),
                title = "Energy",
                type = "NormalCourse",
                difficulty = "Medium",
                status = "Draft"
            });
        var sectionId = section.GetProperty("id").GetInt32();
        var firstAtomic = await PostJsonAsync(
            client,
            "/api/cms-v2/atomic-sections",
            new { sectionId, title = "First Atomic", type = "Custom", difficulty = "Basic", status = "Draft" });
        var secondAtomic = await PostJsonAsync(
            client,
            "/api/cms-v2/atomic-sections",
            new { sectionId, title = "Second Atomic", type = "Custom", difficulty = "Medium", status = "Draft" });
        var firstAtomicId = firstAtomic.GetProperty("id").GetInt32();
        var secondAtomicId = secondAtomic.GetProperty("id").GetInt32();
        var firstSectionItem = await PostJsonAsync(
            client,
            $"/api/cms-v2/sections/{sectionId}/items",
            new
            {
                targetType = "AtomicSection",
                targetId = firstAtomicId,
                referenceMode = "FollowLatest",
                sortOrder = 10,
                status = "Active"
            });
        var secondSectionItem = await PostJsonAsync(
            client,
            $"/api/cms-v2/sections/{sectionId}/items",
            new
            {
                targetType = "AtomicSection",
                targetId = secondAtomicId,
                referenceMode = "FollowLatest",
                sortOrder = 20,
                status = "Active"
            });
        var firstSectionItemId = firstSectionItem.GetProperty("id").GetInt32();
        var secondSectionItemId = secondSectionItem.GetProperty("id").GetInt32();
        var createdBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks",
            new
            {
                sectionId,
                title = string.Empty,
                blockType = "Question",
                difficulty = "Basic",
                status = "Draft"
            });
        var contentBlockId = createdBlock.GetProperty("id").GetInt32();

        await PostJsonAsync(
            client,
            $"/api/cms-v2/atomic-sections/{firstAtomicId}/title",
            new { title = "Renamed Atomic" });
        await PostJsonAsync(
            client,
            $"/api/cms-v2/atomic-sections/{firstAtomicId}/items",
            new { contentBlockId, referenceMode = "FollowLatest", sortOrder = 40 });
        await PostJsonAsync(
            client,
            $"/api/cms-v2/sections/{sectionId}/items/{firstSectionItemId}/move",
            new { direction = "Down" });
        var deleteResponse = await client.DeleteAsync(
            $"/api/cms-v2/sections/{sectionId}/items/{firstSectionItemId}");
        Assert.True(deleteResponse.IsSuccessStatusCode, await deleteResponse.Content.ReadAsStringAsync());

        var renamedAtomic = await client.GetFromJsonAsync<JsonElement>($"/api/cms-v2/atomic-sections/{firstAtomicId}");
        var atomicChildren = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/atomic-sections/{firstAtomicId}/items")
            ?? [];
        var sectionItems = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/sections/{sectionId}/items")
            ?? [];

        Assert.Equal("Renamed Atomic", renamedAtomic.GetProperty("title").GetString());
        Assert.Equal(4, atomicChildren.Length);
        Assert.Contains(
            atomicChildren,
            item => item.GetProperty("contentBlockId").GetInt32() == contentBlockId);
        Assert.Single(sectionItems);
        Assert.Equal(secondSectionItemId, sectionItems[0].GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task Section_items_can_be_wrapped_as_atomic_section_through_api()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "功能关系", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new
            {
                teachingTopicId = topic.GetProperty("id").GetInt32(),
                title = "机械能守恒",
                type = "NormalCourse",
                difficulty = "Medium",
                status = "Draft"
            });
        var sectionId = section.GetProperty("id").GetInt32();
        var firstBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks/blank-document",
            new { sectionId, title = "知识点", blockType = "KnowledgePoint", difficulty = "Basic", status = "Draft" });
        var secondBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks/blank-document",
            new { sectionId, title = "题目", blockType = "Question", difficulty = "Medium", status = "Draft" });
        var firstItem = await PostJsonAsync(
            client,
            $"/api/cms-v2/sections/{sectionId}/items",
            new
            {
                targetType = "ContentBlock",
                targetId = firstBlock.GetProperty("contentBlockId").GetInt32(),
                referenceMode = "FollowLatest",
                sortOrder = 10,
                status = "Active"
            });
        var secondItem = await PostJsonAsync(
            client,
            $"/api/cms-v2/sections/{sectionId}/items",
            new
            {
                targetType = "ContentBlock",
                targetId = secondBlock.GetProperty("contentBlockId").GetInt32(),
                referenceMode = "FollowLatest",
                sortOrder = 20,
                status = "Active"
            });

        var wrapped = await PostJsonAsync(
            client,
            $"/api/cms-v2/sections/{sectionId}/items/wrap-as-atomic-section",
            new
            {
                sectionItemIds = new[] { firstItem.GetProperty("id").GetInt32(), secondItem.GetProperty("id").GetInt32() },
                title = "守恒基础",
                description = "由两个块升级",
                type = "Custom",
                difficulty = "Medium",
                status = "Draft"
            });
        var sectionItems = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/sections/{sectionId}/items")
            ?? [];
        var atomicItems = await client.GetFromJsonAsync<JsonElement[]>(
                $"/api/cms-v2/atomic-sections/{wrapped.GetProperty("atomicSectionId").GetInt32()}/items")
            ?? [];

        Assert.Equal(sectionId, wrapped.GetProperty("sectionId").GetInt32());
        Assert.Equal(2, wrapped.GetProperty("wrappedSectionItemIds").GetArrayLength());
        Assert.Equal(2, wrapped.GetProperty("atomicSectionItemIds").GetArrayLength());
        Assert.Single(sectionItems);
        Assert.Equal("AtomicSection", sectionItems[0].GetProperty("targetType").GetString());
        Assert.Equal(wrapped.GetProperty("atomicSectionId").GetInt32(), sectionItems[0].GetProperty("targetId").GetInt32());
        Assert.Equal(2, atomicItems.Length);

        var invalid = await client.PostAsJsonAsync(
            $"/api/cms-v2/sections/{sectionId}/items/wrap-as-atomic-section",
            new
            {
                sectionItemIds = new[] { sectionItems[0].GetProperty("id").GetInt32() },
                title = "无效",
                type = "Custom",
                difficulty = "Basic",
                status = "Draft"
            });

        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [Fact]
    public async Task Composition_and_handout_endpoints_generate_word_and_expose_generated_files()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var templatePath = Path.Combine(factory.BankRootDirectory, "templates", "default.docx");
        await CreateMinimalDocxAsync(templatePath, "模板正文");
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "机械能", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "机械能专题", type = "NormalCourse", difficulty = "Medium", status = "Draft" });
        var sectionId = section.GetProperty("id").GetInt32();
        var importedBlock = await CreateImportedContentBlockAsync(client, factory.BankRootDirectory, sectionId, "机械能守恒", "讲义正文");
        var sectionItem = await PostJsonAsync(
            client,
            $"/api/cms-v2/sections/{sectionId}/items",
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
        var atomic = await PostJsonAsync(client, "/api/cms-v2/atomic-sections", new { sectionId, title = "原子片段", type = "Custom", difficulty = "Basic", status = "Draft" });
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
            new { sectionId = 1, title = " ", blockType = "KnowledgePoint" });
        var missingBlock = await client.GetAsync("/api/cms-v2/content-blocks/404");
        var missingParent = await client.PostAsJsonAsync(
            "/api/cms-v2/sections",
            new { teachingTopicId = 404, title = "孤立小节" });

        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "Duplicate Section Topic" });
        var topicId = topic.GetProperty("id").GetInt32();
        await PostJsonAsync(client, "/api/cms-v2/sections", new { teachingTopicId = topicId, title = "First Section" });
        var duplicateSection = await client.PostAsJsonAsync(
            "/api/cms-v2/sections",
            new { teachingTopicId = topicId, title = "Duplicate Section" });

        Assert.Equal(HttpStatusCode.BadRequest, invalidBlock.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingBlock.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingParent.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, duplicateSection.StatusCode);
    }

    [Fact]
    public async Task SectionVariant_selection_preview_endpoint_returns_default_candidates()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "功能关系", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "机械能守恒", type = "NormalCourse", difficulty = "Medium", status = "Draft" });
        var sectionId = section.GetProperty("id").GetInt32();
        var basicBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks",
            new { sectionId, title = "基础知识点", blockType = "KnowledgePoint", difficulty = "Basic", status = "Draft" });
        var advancedBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks",
            new { sectionId, title = "提高例题", blockType = "Question", difficulty = "Advanced", status = "Draft" });
        var basicItem = await PostJsonAsync(
            client,
            $"/api/cms-v2/sections/{sectionId}/items",
            new
            {
                targetType = "ContentBlock",
                targetId = basicBlock.GetProperty("id").GetInt32(),
                referenceMode = "FollowLatest",
                sortOrder = 20
            });
        var advancedItem = await PostJsonAsync(
            client,
            $"/api/cms-v2/sections/{sectionId}/items",
            new
            {
                targetType = "ContentBlock",
                targetId = advancedBlock.GetProperty("id").GetInt32(),
                referenceMode = "FollowLatest",
                sortOrder = 10
            });

        var preview = await PostJsonAsync(
            client,
            "/api/cms-v2/section-variants/selection-preview",
            new { sectionId, difficulty = "Medium" });
        var candidates = preview.EnumerateArray().ToArray();
        var invalidDifficulty = await client.PostAsJsonAsync(
            "/api/cms-v2/section-variants/selection-preview",
            new { sectionId, difficulty = "Unset" });
        var missingSection = await client.PostAsJsonAsync(
            "/api/cms-v2/section-variants/selection-preview",
            new { sectionId = 999_999, difficulty = "Basic" });

        Assert.Equal(2, candidates.Length);
        Assert.Equal(advancedItem.GetProperty("id").GetInt32(), candidates[0].GetProperty("sectionItemId").GetInt32());
        Assert.Equal(basicItem.GetProperty("id").GetInt32(), candidates[1].GetProperty("sectionItemId").GetInt32());
        Assert.Equal("ContentBlock", candidates[0].GetProperty("targetType").GetString());
        Assert.Equal("Advanced", candidates[0].GetProperty("resolvedDifficulty").GetString());
        Assert.False(candidates[0].GetProperty("defaultSelected").GetBoolean());
        Assert.Equal("Basic", candidates[1].GetProperty("resolvedDifficulty").GetString());
        Assert.True(candidates[1].GetProperty("defaultSelected").GetBoolean());
        Assert.True(candidates[1].GetProperty("selectable").GetBoolean());
        Assert.Equal(HttpStatusCode.BadRequest, invalidDifficulty.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, missingSection.StatusCode);
    }

    [Fact]
    public async Task SectionVariant_create_endpoint_creates_selected_items_transactionally()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "Variant Topic", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "Variant Section", type = "NormalCourse", difficulty = "Medium", status = "Draft" });
        var sectionId = section.GetProperty("id").GetInt32();
        var firstBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks",
            new { sectionId, title = "First", blockType = "KnowledgePoint", difficulty = "Basic", status = "Draft" });
        var secondBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks",
            new { sectionId, title = "Second", blockType = "Question", difficulty = "Medium", status = "Draft" });
        var firstItem = await PostJsonAsync(
            client,
            $"/api/cms-v2/sections/{sectionId}/items",
            new
            {
                targetType = "ContentBlock",
                targetId = firstBlock.GetProperty("id").GetInt32(),
                referenceMode = "FollowLatest",
                sortOrder = 20
            });
        var secondItem = await PostJsonAsync(
            client,
            $"/api/cms-v2/sections/{sectionId}/items",
            new
            {
                targetType = "ContentBlock",
                targetId = secondBlock.GetProperty("id").GetInt32(),
                referenceMode = "FollowLatest",
                sortOrder = 10
            });

        var created = await PostJsonAsync(
            client,
            "/api/cms-v2/section-variants",
            new
            {
                sectionId,
                title = "Medium Variant",
                type = "Lecture",
                difficulty = "Medium",
                selectedSectionItemIds = new[]
                {
                    firstItem.GetProperty("id").GetInt32(),
                    secondItem.GetProperty("id").GetInt32()
                }
            });
        var variantId = created.GetProperty("id").GetInt32();
        var variant = await client.GetFromJsonAsync<JsonElement>($"/api/cms-v2/section-variants/{variantId}");
        var variantItems = await client.GetFromJsonAsync<JsonElement>($"/api/cms-v2/section-variants/{variantId}/items");
        var emptyVariant = await PostJsonAsync(
            client,
            "/api/cms-v2/section-variants",
            new { sectionId, title = "Empty Variant", type = "Review", difficulty = "Basic", selectedSectionItemIds = Array.Empty<int>() });
        var invalidDifficulty = await client.PostAsJsonAsync(
            "/api/cms-v2/section-variants",
            new { sectionId, title = "Unset Variant", type = "Lecture", difficulty = "Unset", selectedSectionItemIds = Array.Empty<int>() });
        var duplicateTitle = await client.PostAsJsonAsync(
            "/api/cms-v2/section-variants",
            new { sectionId, title = "medium variant", type = "Lecture", difficulty = "Basic", selectedSectionItemIds = Array.Empty<int>() });
        var missingItem = await client.PostAsJsonAsync(
            "/api/cms-v2/section-variants",
            new { sectionId, title = "Missing Item Variant", type = "Lecture", difficulty = "Basic", selectedSectionItemIds = new[] { 999_999 } });

        var items = variantItems.EnumerateArray().ToArray();
        Assert.Equal(SectionVariantStatus.Draft.ToString(), variant.GetProperty("status").GetString());
        Assert.Equal(1, variant.GetProperty("sortOrder").GetInt32());
        Assert.Equal(2, items.Length);
        Assert.Equal(secondItem.GetProperty("id").GetInt32(), items[0].GetProperty("sectionItemId").GetInt32());
        Assert.Equal(firstItem.GetProperty("id").GetInt32(), items[1].GetProperty("sectionItemId").GetInt32());
        Assert.Equal(1, items[0].GetProperty("sortOrder").GetInt32());
        Assert.Equal(2, items[1].GetProperty("sortOrder").GetInt32());
        Assert.True(emptyVariant.GetProperty("id").GetInt32() > 0);
        Assert.Equal(HttpStatusCode.BadRequest, invalidDifficulty.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, duplicateTitle.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, missingItem.StatusCode);
    }

    private static async Task<ImportedContentBlock> CreateImportedContentBlockAsync(
        HttpClient client,
        string bankRootDirectory,
        int sectionId,
        string title,
        string text)
    {
        var created = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks/blank-document",
            new { sectionId, title, blockType = "KnowledgePoint", status = "Draft" });
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
