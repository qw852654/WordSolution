using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Security;
using System.Text;
using System.Text.Json;
using Aspose.Words;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WordSolution.CmsV2.Application.AtomicSections;
using WordSolution.CmsV2.Application.ContentBlocks;
using WordSolution.CmsV2.Application.Handouts;
using WordSolution.CmsV2.Application.SectionVariants;
using WordSolution.CmsV2.Application.Sections;
using WordSolution.CmsV2.Application.Tags;
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
        Assert.Equal("TEST", health.GetProperty("bankKey").GetString());
        Assert.Equal("测试题库", health.GetProperty("bankDisplayName").GetString());
        Assert.Equal("Test", health.GetProperty("bankKind").GetString());
        Assert.Equal(factory.BankRootDirectory, health.GetProperty("bankRootDirectory").GetString());
        Assert.True(File.Exists(Path.Combine(factory.BankRootDirectory, "cms-v2.db")));
        Assert.False(File.Exists(Path.Combine(factory.BankRootDirectory, "question-bank.db")));
        Assert.Contains("KnowledgePoint", enums.GetProperty("contentBlockType").EnumerateArray().Select(x => x.GetString()));
        Assert.Contains("Knowledge", enums.GetProperty("atomicSectionTeachingRole").EnumerateArray().Select(x => x.GetString()));
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<CmsV2DbContext>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ICmsV2UnitOfWork>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ICmsV2FileAssetPathProvider>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IContentBlockFileStore>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IOutputTemplatePathResolver>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IContentBlockDocumentProcessor>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IContentBlockEditSessionStore>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IContentBlockEditSessionFileStore>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IContentBlockEditSessionLauncher>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IHandoutDocumentGenerator>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ContentBlockUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ContentBlockDocumentUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ContentBlockEditSessionUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ContentBlockDeletionUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ContentBlockRelationUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<SectionUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<AtomicSectionUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<SectionVariantUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<TeachingStructureUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<HandoutUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<HandoutGenerationUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<TagUseCases>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<TagBindingUseCases>());
        Assert.Contains(
            factory.Services.GetServices<IHostedService>(),
            service => service.GetType().Name == "ContentBlockEditSessionBackgroundService");
    }

    [Fact]
    public async Task Api_health_supports_legacy_bank_root_directory_configuration()
    {
        await using var factory = new CmsV2ApiFactory(useLegacyConfiguration: true);
        var client = factory.CreateClient();

        var health = await client.GetFromJsonAsync<JsonElement>("/api/cms-v2/health");

        Assert.Equal("ok", health.GetProperty("status").GetString());
        Assert.Equal("LEGACY", health.GetProperty("bankKey").GetString());
        Assert.Equal("当前题库", health.GetProperty("bankDisplayName").GetString());
        Assert.Equal("Test", health.GetProperty("bankKind").GetString());
        Assert.Equal(factory.BankRootDirectory, health.GetProperty("bankRootDirectory").GetString());
        Assert.True(File.Exists(Path.Combine(factory.BankRootDirectory, "cms-v2.db")));
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
    public async Task ContentBlock_version_parts_endpoint_returns_structured_question_parts()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "结构化题目", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "结构化 Section", type = "NormalCourse", difficulty = "Medium", status = "Draft" });
        var sectionId = section.GetProperty("id").GetInt32();
        var contentBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks",
            new
            {
                sectionId,
                title = "题目",
                blockType = "Question",
                difficulty = "Medium",
                status = "Draft"
            });
        var contentBlockId = contentBlock.GetProperty("id").GetInt32();
        var importDocxPath = Path.Combine(factory.BankRootDirectory, "imports", "styled-question.docx");
        CreateStyledQuestionDocx(importDocxPath);

        await using var importStream = File.OpenRead(importDocxPath);
        using var form = new MultipartFormDataContent
        {
            { new StreamContent(importStream), "file", "styled-question.docx" },
            { new StringContent("true"), "setAsCurrent" }
        };
        var importedResponse = await client.PostAsync($"/api/cms-v2/content-blocks/{contentBlockId}/versions/import", form);
        var imported = await ReadSuccessJsonAsync(importedResponse);
        var versionId = imported.GetProperty("contentBlockVersionId").GetInt32();
        var versions = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/content-blocks/{contentBlockId}/versions")
            ?? [];
        var parts = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/content-blocks/{contentBlockId}/versions/{versionId}/parts")
            ?? [];
        var html = await client.GetStringAsync($"/api/cms-v2/content-blocks/{contentBlockId}/versions/{versionId}/html-preview");

        Assert.Contains(versions, version =>
            version.GetProperty("id").GetInt32() == versionId
            && version.GetProperty("partParseStatus").GetString() == "Parsed");
        Assert.Equal(["Stem", "Answer", "Analysis"], parts.Select(part => part.GetProperty("partType").GetString()));
        Assert.Contains("data-question-part=\"Stem\"", html);
    }

    [Fact]
    public async Task Tag_endpoints_manage_tags_bindings_and_content_block_and_filter()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "标签主题", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "标签 Section" });
        var sectionId = section.GetProperty("id").GetInt32();
        var bothBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks",
            new { sectionId, title = "双标签题", blockType = "Question" });
        var singleBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks",
            new { sectionId, title = "单标签题", blockType = "Question" });
        var bothBlockId = bothBlock.GetProperty("id").GetInt32();
        var singleBlockId = singleBlock.GetProperty("id").GetInt32();

        var mechanics = await PostJsonAsync(client, "/api/cms-v2/tags", new { name = "  力学  ", color = "tag-purple" });
        var mechanicsDuplicate = await PostJsonAsync(client, "/api/cms-v2/tags", new { name = "力学" });
        var energy = await PostJsonAsync(client, "/api/cms-v2/tags", new { name = "机械能守恒" });
        var mechanicsId = mechanics.GetProperty("id").GetInt32();
        var energyId = energy.GetProperty("id").GetInt32();
        var invalidCreateColor = await client.PostAsJsonAsync(
            "/api/cms-v2/tags",
            new { name = "InvalidCreateColor", color = "tag-cyan" });

        await PutJsonAsync(
            client,
            "/api/cms-v2/tag-bindings",
            new { targetType = "ContentBlock", targetId = bothBlockId, tagIds = new[] { mechanicsId, energyId, energyId } });
        await PutJsonAsync(
            client,
            "/api/cms-v2/tag-bindings",
            new { targetType = "ContentBlock", targetId = singleBlockId, tagIds = new[] { mechanicsId } });
        await PutJsonAsync(
            client,
            "/api/cms-v2/tag-bindings",
            new { targetType = "Section", targetId = sectionId, tagIds = new[] { mechanicsId } });

        var targetBindings = await client.GetFromJsonAsync<JsonElement[]>(
            $"/api/cms-v2/tag-bindings?targetType=ContentBlock&targetId={bothBlockId}")
            ?? [];
        var filtered = await client.GetFromJsonAsync<JsonElement[]>(
            $"/api/cms-v2/content-blocks?tagIds={mechanicsId}&tagIds={energyId}")
            ?? [];
        var searched = await client.GetFromJsonAsync<JsonElement[]>("/api/cms-v2/tags?keyword=力")
            ?? [];

        Assert.Equal(mechanicsId, mechanicsDuplicate.GetProperty("id").GetInt32());
        Assert.Equal("力学", mechanics.GetProperty("name").GetString());
        Assert.Equal("tag-purple", mechanics.GetProperty("color").GetString());
        Assert.Equal("tag-gray", energy.GetProperty("color").GetString());
        Assert.Equal(HttpStatusCode.BadRequest, invalidCreateColor.StatusCode);
        Assert.Equal([mechanicsId, energyId], targetBindings.Select(binding => binding.GetProperty("tagId").GetInt32()));
        Assert.Equal([bothBlockId], filtered.Select(block => block.GetProperty("id").GetInt32()));
        Assert.Equal([mechanicsId], searched.Select(tag => tag.GetProperty("id").GetInt32()));

        await PostJsonAsync(client, $"/api/cms-v2/tags/{energyId}/archive", new { });

        var archivedSearch = await client.GetFromJsonAsync<JsonElement[]>("/api/cms-v2/tags?keyword=机械能")
            ?? [];
        var archivedBindingResponse = await client.PutAsJsonAsync(
            "/api/cms-v2/tag-bindings",
            new { targetType = "ContentBlock", targetId = singleBlockId, tagIds = new[] { energyId } });
        var missingTargetResponse = await client.PutAsJsonAsync(
            "/api/cms-v2/tag-bindings",
            new { targetType = "AtomicSection", targetId = 999_999, tagIds = new[] { mechanicsId } });

        Assert.Empty(archivedSearch);
        Assert.Equal(HttpStatusCode.BadRequest, archivedBindingResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, missingTargetResponse.StatusCode);

        var restored = await PostJsonAsync(client, $"/api/cms-v2/tags/{energyId}/restore", new { });
        var renamed = await PatchJsonAsync(client, $"/api/cms-v2/tags/{energyId}", new { name = "能量守恒", color = "tag-red" });
        var invalidPatchColor = await client.PatchAsJsonAsync(
            $"/api/cms-v2/tags/{energyId}",
            new { color = "tag-cyan" });
        var sectionBindings = await client.GetFromJsonAsync<JsonElement[]>(
            $"/api/cms-v2/tag-bindings?targetType=Section&targetId={sectionId}")
            ?? [];

        Assert.Equal("Active", restored.GetProperty("status").GetString());
        Assert.Equal("能量守恒", renamed.GetProperty("name").GetString());
        Assert.Equal("tag-red", renamed.GetProperty("color").GetString());
        Assert.Equal(HttpStatusCode.BadRequest, invalidPatchColor.StatusCode);
        Assert.Equal([mechanicsId], sectionBindings.Select(binding => binding.GetProperty("tagId").GetInt32()));
    }

    [Fact]
    public async Task Teaching_note_endpoints_manage_notes_search_and_bindings_without_legacy_fields()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "Teaching note topic", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "Teaching note section" });
        var sectionId = section.GetProperty("id").GetInt32();
        var block = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks",
            new { sectionId, title = "Teaching note block", blockType = "Question" });
        var blockId = block.GetProperty("id").GetInt32();
        var atomicSection = await PostJsonAsync(
            client,
            "/api/cms-v2/atomic-sections",
            new { sectionId, title = "Teaching note atomic section" });
        var atomicSectionId = atomicSection.GetProperty("id").GetInt32();
        var panel = await PostJsonAsync(
            client,
            $"/api/cms-v2/atomic-sections/{atomicSectionId}/panels",
            new { title = "Example panel", teachingRole = "Example", difficulty = "Medium" });
        var panelId = panel.GetProperty("id").GetInt32();
        var atomicItem = await PostJsonAsync(
            client,
            $"/api/cms-v2/atomic-sections/{atomicSectionId}/items",
            new
            {
                contentBlockId = blockId,
                referenceMode = "FollowLatest",
                atomicSectionPanelId = panelId,
                teachingRole = "Example"
            });
        var atomicItemId = atomicItem.GetProperty("id").GetInt32();
        var sectionItem = await PostJsonAsync(
            client,
            $"/api/cms-v2/sections/{sectionId}/items",
            new
            {
                targetType = "ContentBlock",
                targetId = blockId,
                referenceMode = "FollowLatest",
                sortOrder = 0
            });
        var sectionItemId = sectionItem.GetProperty("id").GetInt32();

        var created = await PostJsonAsync(
            client,
            "/api/cms-v2/teaching-notes",
            new
            {
                noteType = "ClassroomRecord",
                content = "  pacing was too fast  ",
                effectLevel = (string?)null,
                occurredAt = "2026-06-01T08:00:00Z",
                bindings = new[]
                {
                    new { targetType = "ContentBlock", targetId = blockId },
                    new { targetType = "Section", targetId = sectionId },
                    new { targetType = "AtomicSection", targetId = atomicSectionId },
                    new { targetType = "AtomicSectionPanel", targetId = panelId },
                    new { targetType = "AtomicSectionItem", targetId = atomicItemId },
                    new { targetType = "SectionItem", targetId = sectionItemId }
                }
            });
        var noteId = created.GetProperty("id").GetInt32();
        AssertTeachingNoteHasNoLegacyFields(created);
        Assert.Equal("pacing was too fast", created.GetProperty("content").GetString());
        Assert.Equal(JsonValueKind.Null, created.GetProperty("effectLevel").ValueKind);
        Assert.Equal(6, created.GetProperty("bindings").GetArrayLength());

        var byId = await client.GetFromJsonAsync<JsonElement>($"/api/cms-v2/teaching-notes/{noteId}");
        var byBinding = await client.GetFromJsonAsync<JsonElement[]>(
                $"/api/cms-v2/teaching-note-bindings?targetType=AtomicSectionItem&targetId={atomicItemId}")
            ?? [];
        var search = await client.GetFromJsonAsync<JsonElement[]>(
                "/api/cms-v2/teaching-notes?keyword=pacing&targetType=ContentBlock")
            ?? [];

        Assert.Equal(noteId, byId.GetProperty("id").GetInt32());
        Assert.Equal([noteId], byBinding.Select(note => note.GetProperty("id").GetInt32()));
        Assert.Equal([noteId], search.Select(note => note.GetProperty("id").GetInt32()));

        var updated = await PatchJsonAsync(
            client,
            $"/api/cms-v2/teaching-notes/{noteId}",
            new
            {
                noteType = "RevisionSuggestion",
                content = "add one warmup problem next time",
                effectLevel = "Weak",
                occurredAt = (string?)null,
                bindings = new[]
                {
                    new { targetType = "SectionItem", targetId = sectionItemId }
                }
            });
        var noLongerOnContentBlock = await client.GetFromJsonAsync<JsonElement[]>(
                $"/api/cms-v2/teaching-note-bindings?targetType=ContentBlock&targetId={blockId}")
            ?? [];
        var filteredByEffect = await client.GetFromJsonAsync<JsonElement[]>(
                "/api/cms-v2/teaching-notes?effectLevel=Weak&targetType=SectionItem")
            ?? [];

        Assert.Equal("RevisionSuggestion", updated.GetProperty("noteType").GetString());
        Assert.Equal("Weak", updated.GetProperty("effectLevel").GetString());
        Assert.Single(updated.GetProperty("bindings").EnumerateArray());
        Assert.Empty(noLongerOnContentBlock);
        Assert.Equal([noteId], filteredByEffect.Select(note => note.GetProperty("id").GetInt32()));

        var deleteResponse = await client.DeleteAsync($"/api/cms-v2/teaching-notes/{noteId}");
        var deletedGetResponse = await client.GetAsync($"/api/cms-v2/teaching-notes/{noteId}");
        var deletedBindingQuery = await client.GetFromJsonAsync<JsonElement[]>(
                $"/api/cms-v2/teaching-note-bindings?targetType=SectionItem&targetId={sectionItemId}")
            ?? [];

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deletedGetResponse.StatusCode);
        Assert.Empty(deletedBindingQuery);
    }

    [Fact]
    public async Task Question_import_session_endpoints_parse_candidates_and_batch_confirm()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "多题导入", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "多题导入 Section", type = "NormalCourse", difficulty = "Medium", status = "Draft" });
        var sectionId = section.GetProperty("id").GetInt32();
        var createSessionResponse = await client.PostAsJsonAsync(
            "/api/cms-v2/question-import-sessions",
            new
            {
                context = new
                {
                    sectionId,
                    atomicSectionId = (int?)null,
                    atomicSectionPanelId = (int?)null,
                    afterAtomicSectionItemId = (int?)null,
                    afterSectionItemId = (int?)null,
                    defaultTeachingRole = "Unclassified",
                    defaultDifficulty = "Medium"
                },
                openWord = false
            });
        var createdSession = await ReadSuccessJsonAsync(createSessionResponse);
        var sessionId = createdSession.GetProperty("sessionId").GetString()!;
        var importDocxPath = Path.Combine(factory.BankRootDirectory, "edit-sessions", "question-imports", sessionId, "source.docx");
        CreateMultiQuestionImportDocx(importDocxPath);

        var session = await client.GetFromJsonAsync<JsonElement>($"/api/cms-v2/question-import-sessions/{sessionId}");
        var candidatesFromEndpoint = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/question-import-sessions/{sessionId}/candidates")
            ?? [];
        var candidates = session.GetProperty("candidates").EnumerateArray().ToArray();

        var confirmed = await PostJsonAsync(
            client,
            $"/api/cms-v2/question-import-sessions/{sessionId}/confirm",
            new
            {
                candidates = candidates.Select((candidate, index) => new
                {
                    candidateId = candidate.GetProperty("candidateId").GetString(),
                    selected = index != 1,
                    title = index == 0 ? "确认导入题目" : string.Empty
                }).ToArray()
            });
        var contentBlockIds = confirmed.GetProperty("contentBlockIds").EnumerateArray().Select(item => item.GetInt32()).ToArray();
        var versionIds = confirmed.GetProperty("contentBlockVersionIds").EnumerateArray().Select(item => item.GetInt32()).ToArray();
        var sectionItemIds = confirmed.GetProperty("sectionItemIds").EnumerateArray().Select(item => item.GetInt32()).ToArray();
        var parts = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/content-blocks/{contentBlockIds[0]}/versions/{versionIds[0]}/parts")
            ?? [];
        var sectionItems = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/sections/{sectionId}/items")
            ?? [];

        Assert.Equal("ReadyForReview", session.GetProperty("status").GetString());
        Assert.Equal(3, candidates.Length);
        Assert.Equal(3, candidatesFromEndpoint.Length);
        Assert.All(candidates, candidate =>
        {
            Assert.Equal("Parsed", candidate.GetProperty("parseStatus").GetString());
            Assert.Contains("data-question-part=\"Stem\"", candidate.GetProperty("htmlPreview").GetString());
        });
        Assert.DoesNotContain("导入前说明", string.Join("\n", candidates.Select(candidate => candidate.GetProperty("htmlPreview").GetString())));
        Assert.Equal(2, contentBlockIds.Length);
        Assert.Equal(2, versionIds.Length);
        Assert.Equal(2, sectionItemIds.Length);
        Assert.Equal("SectionItem", confirmed.GetProperty("firstInsertedNodeType").GetString());
        Assert.Equal(2, sectionItems.Length);
        Assert.Equal(["Stem", "Answer"], parts.Select(part => part.GetProperty("partType").GetString()));
    }

    [Fact]
    public async Task ContentBlock_delete_cascade_endpoint_removes_block_versions_and_section_variant_references()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "删除测试", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "删除测试 Section", type = "NormalCourse", difficulty = "Medium", status = "Draft" });
        var sectionId = section.GetProperty("id").GetInt32();
        var created = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks/blank-document",
            new
            {
                sectionId,
                title = "待彻底删除",
                blockType = "KnowledgePoint",
                difficulty = "Basic",
                status = "Draft"
            });
        var contentBlockId = created.GetProperty("contentBlockId").GetInt32();
        var sectionItem = await PostJsonAsync(
            client,
            $"/api/cms-v2/sections/{sectionId}/items",
            new
            {
                targetType = "ContentBlock",
                targetId = contentBlockId,
                referenceMode = "FollowLatest",
                lockedContentBlockVersionId = (int?)null,
                sortOrder = 10,
                status = "Draft"
            });
        var variant = await PostJsonAsync(
            client,
            "/api/cms-v2/section-variants",
            new
            {
                sectionId,
                title = "引用待删块",
                type = "Lecture",
                difficulty = "Basic",
                selectedSectionItemIds = new[] { sectionItem.GetProperty("id").GetInt32() }
            });

        var deleteResult = await PostJsonAsync(
            client,
            $"/api/cms-v2/content-blocks/{contentBlockId}/delete-cascade",
            new { });
        var deletedBlock = await client.GetAsync($"/api/cms-v2/content-blocks/{contentBlockId}");
        var variantItems = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/section-variants/{variant.GetProperty("id").GetInt32()}/items")
            ?? [];

        Assert.Equal(contentBlockId, deleteResult.GetProperty("contentBlockId").GetInt32());
        Assert.Equal(1, deleteResult.GetProperty("removedSectionItemCount").GetInt32());
        Assert.Equal(1, deleteResult.GetProperty("removedSectionVariantItemCount").GetInt32());
        Assert.Equal(1, deleteResult.GetProperty("removedVersionCount").GetInt32());
        Assert.True(deleteResult.GetProperty("deletedAssetCount").GetInt32() > 0);
        Assert.Equal(HttpStatusCode.NotFound, deletedBlock.StatusCode);
        Assert.Empty(variantItems);
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
    public async Task Atomic_section_panel_endpoints_create_classify_and_delete_panel_without_deleting_blocks()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "Atomic panels", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new
            {
                teachingTopicId = topic.GetProperty("id").GetInt32(),
                title = "Panel section",
                type = "NormalCourse",
                difficulty = "Medium",
                status = "Draft"
            });
        var sectionId = section.GetProperty("id").GetInt32();
        var atomicSection = await PostJsonAsync(
            client,
            "/api/cms-v2/atomic-sections",
            new
            {
                sectionId,
                title = "Panel AS",
                type = "Custom",
                difficulty = "Top",
                status = "Draft"
            });
        var atomicSectionId = atomicSection.GetProperty("id").GetInt32();
        var block = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks",
            new
            {
                sectionId,
                title = "Example block",
                blockType = "Question",
                difficulty = "Basic",
                status = "Draft"
            });
        var blockId = block.GetProperty("id").GetInt32();

        var panel = await PostJsonAsync(
            client,
            $"/api/cms-v2/atomic-sections/{atomicSectionId}/panels",
            new
            {
                title = "Example basic",
                teachingRole = "Example",
                difficulty = "Basic"
            });
        var panelId = panel.GetProperty("id").GetInt32();
        var mediumPanel = await PostJsonAsync(
            client,
            $"/api/cms-v2/atomic-sections/{atomicSectionId}/panels",
            new
            {
                title = "Example medium",
                teachingRole = "Example",
                difficulty = "Medium"
            });
        var mediumPanelId = mediumPanel.GetProperty("id").GetInt32();
        var item = await PostJsonAsync(
            client,
            $"/api/cms-v2/atomic-sections/{atomicSectionId}/items",
            new
            {
                contentBlockId = blockId,
                referenceMode = "FollowLatest",
                sortOrder = 10,
                atomicSectionPanelId = panelId,
                teachingRole = "Example"
            });
        var items = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/atomic-sections/{atomicSectionId}/items")
            ?? [];
        var updatedBlock = await PostJsonAsync(
            client,
            $"/api/cms-v2/content-blocks/{blockId}/difficulty",
            new { difficulty = "Medium" });
        var updatedAtomicSection = await PostJsonAsync(
            client,
            $"/api/cms-v2/atomic-sections/{atomicSectionId}/difficulty",
            new { difficulty = "Advanced" });
        var itemsAfterDifficultyChange = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/atomic-sections/{atomicSectionId}/items")
            ?? [];
        var deletePanel = await client.DeleteAsync($"/api/cms-v2/atomic-sections/{atomicSectionId}/panels/{mediumPanelId}");
        var deleted = await ReadSuccessJsonAsync(deletePanel);
        var contentBlockAfterDelete = await client.GetAsync($"/api/cms-v2/content-blocks/{blockId}");

        Assert.Equal("Example", panel.GetProperty("teachingRole").GetString());
        Assert.Single(items);
        Assert.Equal(item.GetProperty("id").GetInt32(), items[0].GetProperty("id").GetInt32());
        Assert.Equal(panelId, items[0].GetProperty("atomicSectionPanelId").GetInt32());
        Assert.Equal("Example", items[0].GetProperty("teachingRole").GetString());
        Assert.Equal("Medium", updatedBlock.GetProperty("difficulty").GetString());
        Assert.Equal("Advanced", updatedAtomicSection.GetProperty("difficulty").GetString());
        Assert.Single(itemsAfterDifficultyChange);
        Assert.Equal(mediumPanelId, itemsAfterDifficultyChange[0].GetProperty("atomicSectionPanelId").GetInt32());
        Assert.Equal(1, deleted.GetProperty("removedAtomicSectionItemCount").GetInt32());
        Assert.Equal(HttpStatusCode.OK, contentBlockAfterDelete.StatusCode);
    }

    [Fact]
    public async Task AtomicSection_status_endpoint_updates_status()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "Atomic status topic", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "Atomic status section" });
        var sectionId = section.GetProperty("id").GetInt32();
        var atomic = await PostJsonAsync(
            client,
            "/api/cms-v2/atomic-sections",
            new
            {
                sectionId,
                title = "Status AS",
                type = "Custom",
                difficulty = "Basic",
                status = "Draft"
            });

        var response = await PostJsonAsync(
            client,
            $"/api/cms-v2/atomic-sections/{atomic.GetProperty("id").GetInt32()}/status",
            new { status = "Active" });

        Assert.Equal("Active", response.GetProperty("status").GetString());
    }

    [Fact]
    public async Task AtomicSection_create_endpoint_creates_default_panels_without_default_items()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "Atomic default panels topic", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "Atomic default panels section" });
        var sectionId = section.GetProperty("id").GetInt32();
        var atomic = await PostJsonAsync(
            client,
            "/api/cms-v2/atomic-sections",
            new
            {
                sectionId,
                title = "Default panel AS",
                type = "Custom",
                difficulty = "Advanced",
                status = "Draft"
            });
        var atomicSectionId = atomic.GetProperty("id").GetInt32();

        var panels = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/atomic-sections/{atomicSectionId}/panels")
            ?? [];
        var items = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/atomic-sections/{atomicSectionId}/items")
            ?? [];

        Assert.Equal(["Knowledge", "Example", "Variant"], panels.Select(panel => panel.GetProperty("teachingRole").GetString()));
        Assert.All(panels, panel => Assert.Equal("Default panel AS", panel.GetProperty("title").GetString()));
        Assert.All(panels, panel => Assert.Equal("Advanced", panel.GetProperty("difficulty").GetString()));
        Assert.Equal([10, 20, 30], panels.Select(panel => panel.GetProperty("sortOrder").GetInt32()));
        Assert.Empty(items);
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
        Assert.Single(atomicChildren);
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
        var generatedFilePath = generated.GetProperty("filePath").GetString();
        var generatedFiles = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/output-forms/{outputForm.GetProperty("id").GetInt32()}/generated-files")
            ?? [];
        var manifest = await client.GetFromJsonAsync<JsonElement>($"/api/cms-v2/generated-files/{generatedFileId}/manifest");
        var download = await client.GetAsync($"/api/cms-v2/generated-files/{generatedFileId}/download");

        Assert.True(File.Exists(generatedFilePath));
        Assert.Single(generatedFiles);
        Assert.Equal(generatedFileId, generatedFiles[0].GetProperty("id").GetInt32());
        Assert.Equal(1, manifest.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(importedBlock.ImportedVersionId, manifest.GetProperty("sources")[0].GetProperty("contentBlockVersionId").GetInt32());
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.NotEmpty(await download.Content.ReadAsByteArrayAsync());

        var delete = await client.DeleteAsync($"/api/cms-v2/generated-files/{generatedFileId}");
        var generatedFilesAfterDelete = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/output-forms/{outputForm.GetProperty("id").GetInt32()}/generated-files")
            ?? [];
        var manifestAfterDelete = await client.GetAsync($"/api/cms-v2/generated-files/{generatedFileId}/manifest");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Empty(generatedFilesAfterDelete);
        Assert.Equal(HttpStatusCode.NotFound, manifestAfterDelete.StatusCode);
        Assert.False(File.Exists(generatedFilePath));
    }

    [Fact]
    public async Task Output_form_validate_word_generation_endpoint_returns_structured_issues_without_generating_file()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var templatePath = Path.Combine(factory.BankRootDirectory, "templates", "missing-style-template.docx");
        await CreateMinimalDocxAsync(templatePath, "模板正文");
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "输出预检", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "预检小节", type = "NormalCourse", difficulty = "Medium", status = "Draft" });
        var contentBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks/blank-document",
            new
            {
                sectionId = section.GetProperty("id").GetInt32(),
                title = "预检题目",
                blockType = "Question",
                questionType = "Unset",
                status = "Draft"
            });
        var importDocxPath = Path.Combine(factory.BankRootDirectory, "imports", "validate-question.docx");
        CreateStyledQuestionDocx(importDocxPath);
        await using (var importStream = File.OpenRead(importDocxPath))
        using (var form = new MultipartFormDataContent
        {
            { new StreamContent(importStream), "file", "source.docx" },
            { new StringContent("true"), "setAsCurrent" }
        })
        {
            await ReadSuccessJsonAsync(await client.PostAsync(
                $"/api/cms-v2/content-blocks/{contentBlock.GetProperty("contentBlockId").GetInt32()}/versions/import",
                form));
        }

        var handout = await PostJsonAsync(client, "/api/cms-v2/handouts", new { title = "预检讲义", status = "Draft" });
        var handoutVersion = await PostJsonAsync(
            client,
            $"/api/cms-v2/handouts/{handout.GetProperty("id").GetInt32()}/versions",
            new { title = "预检版本", type = "Normal", status = "Draft", sortOrder = 1 });
        await PostJsonAsync(
            client,
            $"/api/cms-v2/handout-versions/{handoutVersion.GetProperty("id").GetInt32()}/items",
            new { targetType = "ContentBlock", targetId = contentBlock.GetProperty("contentBlockId").GetInt32(), sortOrder = 1 });
        var template = await PostJsonAsync(
            client,
            "/api/cms-v2/output-templates",
            new { title = "缺样式模板", templateDocxPath = templatePath, status = "Active" });
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
        var outputFormId = outputForm.GetProperty("id").GetInt32();

        var validationResponse = await client.PostAsync(
            $"/api/cms-v2/output-forms/{outputFormId}/validate-word-generation",
            content: null);
        var validation = JsonDocument.Parse(await validationResponse.Content.ReadAsStringAsync()).RootElement.Clone();
        var generateResponse = await client.PostAsJsonAsync(
            $"/api/cms-v2/output-forms/{outputFormId}/generate-word",
            new { generatedTime = "2026-06-09T00:00:00Z" });
        var generatedFiles = await client.GetFromJsonAsync<JsonElement[]>($"/api/cms-v2/output-forms/{outputFormId}/generated-files")
            ?? [];

        Assert.Equal(HttpStatusCode.OK, validationResponse.StatusCode);
        Assert.False(validation.GetProperty("isValid").GetBoolean());
        var issue = Assert.Single(validation.GetProperty("issues").EnumerateArray());
        Assert.Equal("MissingOutputStyle", issue.GetProperty("code").GetString());
        Assert.Equal(outputFormId, issue.GetProperty("outputFormId").GetInt32());
        Assert.Equal(template.GetProperty("id").GetInt32(), issue.GetProperty("outputTemplateId").GetInt32());
        Assert.Equal("练习题", issue.GetProperty("requiredStyleName").GetString());
        Assert.Equal(HttpStatusCode.BadRequest, generateResponse.StatusCode);
        Assert.Empty(generatedFiles);
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
    public async Task Output_template_validate_endpoint_reports_docx_path_status_without_creating_template()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var templatePath = Path.Combine(factory.BankRootDirectory, "templates", "valid-template.docx");
        var missingTemplatePath = Path.Combine(factory.BankRootDirectory, "templates", "missing-template.docx");
        var nonDocxPath = Path.Combine(factory.BankRootDirectory, "templates", "template.txt");
        await CreateMinimalDocxAsync(templatePath, "Template");
        Directory.CreateDirectory(Path.GetDirectoryName(nonDocxPath)!);
        await File.WriteAllTextAsync(nonDocxPath, "not a docx");

        var valid = await PostJsonAsync(
            client,
            "/api/cms-v2/output-templates/validate",
            new { templateDocxPath = templatePath });
        var missing = await PostJsonAsync(
            client,
            "/api/cms-v2/output-templates/validate",
            new { templateDocxPath = missingTemplatePath });
        var nonDocx = await PostJsonAsync(
            client,
            "/api/cms-v2/output-templates/validate",
            new { templateDocxPath = nonDocxPath });
        var templates = await client.GetFromJsonAsync<JsonElement[]>("/api/cms-v2/output-templates")
            ?? [];

        Assert.True(valid.GetProperty("valid").GetBoolean());
        Assert.Contains("ready", valid.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.False(missing.GetProperty("valid").GetBoolean());
        Assert.Contains("not found", missing.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.False(nonDocx.GetProperty("valid").GetBoolean());
        Assert.Contains(".docx", nonDocx.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.Empty(templates);
    }

    [Fact]
    public async Task Output_template_validate_endpoint_accepts_legacy_default_template_path_from_runtime_output_directory()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var runtimeTemplatePath = Path.Combine(
            AppContext.BaseDirectory,
            "Documents",
            "Templates",
            "content-block-default.docx");
        if (!File.Exists(runtimeTemplatePath))
        {
            await CreateMinimalDocxAsync(runtimeTemplatePath, "Runtime default template");
        }

        var valid = await PostJsonAsync(
            client,
            "/api/cms-v2/output-templates/validate",
            new
            {
                templateDocxPath = "src-v2/WordSolution.CmsV2.Infrastructure/Documents/Templates/content-block-default.docx"
            });

        Assert.True(valid.GetProperty("valid").GetBoolean());
        Assert.Contains("ready", valid.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
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

    [Fact]
    public async Task SectionVariant_delete_endpoint_removes_variant_and_items()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "Variant Delete Topic", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "Variant Delete Section", type = "NormalCourse", difficulty = "Medium", status = "Draft" });
        var sectionId = section.GetProperty("id").GetInt32();
        var block = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks",
            new { sectionId, title = "Delete Variant Item", blockType = "KnowledgePoint", difficulty = "Basic", status = "Draft" });
        var sectionItem = await PostJsonAsync(
            client,
            $"/api/cms-v2/sections/{sectionId}/items",
            new
            {
                targetType = "ContentBlock",
                targetId = block.GetProperty("id").GetInt32(),
                referenceMode = "FollowLatest",
                sortOrder = 1
            });
        var created = await PostJsonAsync(
            client,
            "/api/cms-v2/section-variants",
            new
            {
                sectionId,
                title = "Variant To Delete",
                type = "Lecture",
                difficulty = "Basic",
                selectedSectionItemIds = new[] { sectionItem.GetProperty("id").GetInt32() }
            });
        var variantId = created.GetProperty("id").GetInt32();

        var deleteResponse = await client.DeleteAsync($"/api/cms-v2/section-variants/{variantId}");
        var getDeletedResponse = await client.GetAsync($"/api/cms-v2/section-variants/{variantId}");
        var variantItems = await client.GetFromJsonAsync<JsonElement>($"/api/cms-v2/section-variants/{variantId}/items");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);
        Assert.Empty(variantItems.EnumerateArray());
    }

    [Fact]
    public async Task Handout_version_item_endpoints_add_after_move_patch_and_delete_items()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "Handout Item Topic", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "Handout Item Section", type = "NormalCourse", difficulty = "Medium", status = "Draft" });
        var sectionId = section.GetProperty("id").GetInt32();
        var firstBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks",
            new { sectionId, title = "First", blockType = "KnowledgePoint", difficulty = "Basic", status = "Draft" });
        var insertedBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks",
            new { sectionId, title = "Inserted", blockType = "Question", difficulty = "Medium", status = "Draft" });
        var lastBlock = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks",
            new { sectionId, title = "Last", blockType = "ExerciseGroup", difficulty = "Advanced", status = "Draft" });
        var handout = await PostJsonAsync(client, "/api/cms-v2/handouts", new { title = "Handout Item Flow", status = "Draft" });
        var handoutVersion = await PostJsonAsync(
            client,
            $"/api/cms-v2/handouts/{handout.GetProperty("id").GetInt32()}/versions",
            new { title = "Editable Version", type = "Normal", status = "Draft" });
        var handoutVersionId = handoutVersion.GetProperty("id").GetInt32();

        var first = await PostJsonAsync(
            client,
            $"/api/cms-v2/handout-versions/{handoutVersionId}/items",
            new { targetType = "ContentBlock", targetId = firstBlock.GetProperty("id").GetInt32() });
        var last = await PostJsonAsync(
            client,
            $"/api/cms-v2/handout-versions/{handoutVersionId}/items",
            new { targetType = "ContentBlock", targetId = lastBlock.GetProperty("id").GetInt32() });
        var inserted = await PostJsonAsync(
            client,
            $"/api/cms-v2/handout-versions/{handoutVersionId}/items",
            new
            {
                targetType = "ContentBlock",
                targetId = insertedBlock.GetProperty("id").GetInt32(),
                afterHandoutVersionItemId = first.GetProperty("id").GetInt32()
            });

        await PatchJsonAsync(
            client,
            $"/api/cms-v2/handout-versions/{handoutVersionId}/items/{inserted.GetProperty("id").GetInt32()}",
            new { titleOverride = "Inserted override", note = "Inserted note" });
        await PostJsonAsync(
            client,
            $"/api/cms-v2/handout-versions/{handoutVersionId}/items/{last.GetProperty("id").GetInt32()}/move",
            new { direction = "Up" });
        var deleteResponse = await client.DeleteAsync(
            $"/api/cms-v2/handout-versions/{handoutVersionId}/items/{inserted.GetProperty("id").GetInt32()}");
        var items = await client.GetFromJsonAsync<JsonElement[]>(
                $"/api/cms-v2/handout-versions/{handoutVersionId}/items")
            ?? [];

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal([first.GetProperty("id").GetInt32(), last.GetProperty("id").GetInt32()], items.Select(item => item.GetProperty("id").GetInt32()));
        Assert.Equal([10, 20], items.Select(item => item.GetProperty("sortOrder").GetInt32()));
        Assert.DoesNotContain(items, item => item.GetProperty("id").GetInt32() == inserted.GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task Handout_version_workspace_endpoint_returns_aggregate()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "Workspace Topic", sortOrder = 1 });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "Workspace Section", type = "NormalCourse", difficulty = "Medium", status = "Draft" });
        var block = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks",
            new { sectionId = section.GetProperty("id").GetInt32(), title = "Workspace Block", blockType = "KnowledgePoint", difficulty = "Basic", status = "Draft" });
        var handout = await PostJsonAsync(client, "/api/cms-v2/handouts", new { title = "Workspace Handout", status = "Draft" });
        var handoutVersion = await PostJsonAsync(
            client,
            $"/api/cms-v2/handouts/{handout.GetProperty("id").GetInt32()}/versions",
            new { title = "Workspace Version", type = "Normal", status = "Draft" });
        var handoutVersionId = handoutVersion.GetProperty("id").GetInt32();
        var item = await PostJsonAsync(
            client,
            $"/api/cms-v2/handout-versions/{handoutVersionId}/items",
            new { targetType = "ContentBlock", targetId = block.GetProperty("id").GetInt32() });

        var version = await client.GetFromJsonAsync<JsonElement>($"/api/cms-v2/handout-versions/{handoutVersionId}");
        var workspace = await client.GetFromJsonAsync<JsonElement>($"/api/cms-v2/handout-versions/{handoutVersionId}/workspace");
        var items = workspace.GetProperty("items").EnumerateArray().ToArray();
        var outputForms = workspace.GetProperty("outputForms").EnumerateArray().ToArray();

        Assert.Equal(handoutVersionId, version.GetProperty("id").GetInt32());
        Assert.Equal(handout.GetProperty("id").GetInt32(), workspace.GetProperty("handout").GetProperty("id").GetInt32());
        Assert.Equal(handoutVersionId, workspace.GetProperty("version").GetProperty("id").GetInt32());
        Assert.Single(items);
        Assert.Equal(item.GetProperty("id").GetInt32(), items[0].GetProperty("handoutVersionItemId").GetInt32());
        Assert.Equal("ContentBlock", items[0].GetProperty("targetType").GetString());
        Assert.Equal("Workspace Block", items[0].GetProperty("title").GetString());
        var outputForm = Assert.Single(outputForms);
        Assert.Equal("课堂 Word", outputForm.GetProperty("title").GetString());
        Assert.Equal("Word", outputForm.GetProperty("outputFormat").GetString());
        Assert.Equal("Classroom", outputForm.GetProperty("visibilityMode").GetString());
        Assert.Empty(workspace.GetProperty("generatedFiles").EnumerateArray());
    }

    [Fact]
    public async Task Handout_management_endpoints_update_archive_and_enforce_write_guards()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var handout = await PostJsonAsync(client, "/api/cms-v2/handouts", new { title = "讲义管理", status = "Draft" });
        var handoutId = handout.GetProperty("id").GetInt32();
        await PatchJsonAsync(
            client,
            $"/api/cms-v2/handouts/{handoutId}",
            new { title = "讲义管理改名", description = "更新说明", status = "Active" });
        var updatedHandout = await client.GetFromJsonAsync<JsonElement>($"/api/cms-v2/handouts/{handoutId}");

        Assert.Equal("讲义管理改名", updatedHandout.GetProperty("title").GetString());
        Assert.Equal("更新说明", updatedHandout.GetProperty("description").GetString());
        Assert.Equal("Active", updatedHandout.GetProperty("status").GetString());

        var version = await PostJsonAsync(
            client,
            $"/api/cms-v2/handouts/{handoutId}/versions",
            new { title = "基础版", type = "Normal", status = "Active", sortOrder = 999 });
        var versionId = version.GetProperty("id").GetInt32();
        var loadedVersion = await client.GetFromJsonAsync<JsonElement>($"/api/cms-v2/handout-versions/{versionId}");
        Assert.Equal(10, loadedVersion.GetProperty("sortOrder").GetInt32());
        Assert.Equal("Draft", loadedVersion.GetProperty("status").GetString());

        await PatchJsonAsync(
            client,
            $"/api/cms-v2/handout-versions/{versionId}",
            new { title = "基础版归档", type = "Normal", status = "Archived", sortOrder = 10 });

        var topic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "讲义管理主题" });
        var section = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = topic.GetProperty("id").GetInt32(), title = "讲义管理小节" });
        var block = await PostJsonAsync(
            client,
            "/api/cms-v2/content-blocks",
            new { sectionId = section.GetProperty("id").GetInt32(), title = "讲义管理内容", blockType = "KnowledgePoint" });
        var archivedWrite = await client.PostAsJsonAsync(
            $"/api/cms-v2/handout-versions/{versionId}/items",
            new { targetType = "ContentBlock", targetId = block.GetProperty("id").GetInt32() });

        Assert.Equal(HttpStatusCode.BadRequest, archivedWrite.StatusCode);
    }

    [Fact]
    public async Task Section_variant_tree_and_batch_add_endpoints_support_handout_selection_flow()
    {
        await using var factory = new CmsV2ApiFactory();
        var client = factory.CreateClient();
        var firstTopic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "Topic A", sortOrder = 10 });
        var secondTopic = await PostJsonAsync(client, "/api/cms-v2/teaching-topics", new { name = "Topic B", sortOrder = 20 });
        var firstSection = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = firstTopic.GetProperty("id").GetInt32(), title = "Section A" });
        var secondSection = await PostJsonAsync(
            client,
            "/api/cms-v2/sections",
            new { teachingTopicId = secondTopic.GetProperty("id").GetInt32(), title = "Section B" });
        var firstVariant = await PostJsonAsync(
            client,
            "/api/cms-v2/section-variants",
            new
            {
                sectionId = firstSection.GetProperty("id").GetInt32(),
                title = "Variant A",
                type = "Lecture",
                difficulty = "Basic",
                selectedSectionItemIds = Array.Empty<int>()
            });
        var secondVariant = await PostJsonAsync(
            client,
            "/api/cms-v2/section-variants",
            new
            {
                sectionId = secondSection.GetProperty("id").GetInt32(),
                title = "Variant B",
                type = "Lecture",
                difficulty = "Basic",
                selectedSectionItemIds = Array.Empty<int>()
            });
        var handout = await PostJsonAsync(client, "/api/cms-v2/handouts", new { title = "Batch Variant Handout" });
        var version = await PostJsonAsync(
            client,
            $"/api/cms-v2/handouts/{handout.GetProperty("id").GetInt32()}/versions",
            new { title = "Batch Version" });
        var handoutVersionId = version.GetProperty("id").GetInt32();
        var existing = await PostJsonAsync(
            client,
            $"/api/cms-v2/handout-versions/{handoutVersionId}/items",
            new
            {
                targetType = "SectionVariant",
                targetId = secondVariant.GetProperty("id").GetInt32()
            });

        var tree = await client.GetFromJsonAsync<JsonElement[]>("/api/cms-v2/section-variants/tree") ?? [];
        var batchResult = await PostJsonAsync(
            client,
            $"/api/cms-v2/handout-versions/{handoutVersionId}/items/batch-add-section-variants",
            new
            {
                sectionVariantIds = new[]
                {
                    secondVariant.GetProperty("id").GetInt32(),
                    firstVariant.GetProperty("id").GetInt32()
                },
                insertAfterHandoutVersionItemId = existing.GetProperty("id").GetInt32()
            });
        var items = await client.GetFromJsonAsync<JsonElement[]>(
                $"/api/cms-v2/handout-versions/{handoutVersionId}/items")
            ?? [];
        var orderedItems = items.OrderBy(item => item.GetProperty("sortOrder").GetInt32()).ToArray();
        var firstTopicSection = tree[0].GetProperty("sections").EnumerateArray().First();

        Assert.Equal(2, tree.Length);
        Assert.Equal(firstTopic.GetProperty("id").GetInt32(), tree[0].GetProperty("teachingTopic").GetProperty("id").GetInt32());
        Assert.Equal(
            firstVariant.GetProperty("id").GetInt32(),
            firstTopicSection.GetProperty("sectionVariants").EnumerateArray().First().GetProperty("id").GetInt32());
        Assert.Single(batchResult.GetProperty("createdItemIds").EnumerateArray());
        Assert.Equal([secondVariant.GetProperty("id").GetInt32()], batchResult.GetProperty("skippedExistingVariantIds").EnumerateArray().Select(item => item.GetInt32()));
        Assert.Equal(
            [secondVariant.GetProperty("id").GetInt32(), firstVariant.GetProperty("id").GetInt32()],
            orderedItems.Select(item => item.GetProperty("targetId").GetInt32()));
        Assert.Equal([10, 20], orderedItems.Select(item => item.GetProperty("sortOrder").GetInt32()));
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

    private static async Task<JsonElement> PatchJsonAsync(HttpClient client, string uri, object value)
    {
        var response = await client.PatchAsJsonAsync(uri, value);
        return await ReadSuccessJsonAsync(response);
    }

    private static async Task<JsonElement> PutJsonAsync(HttpClient client, string uri, object value)
    {
        var response = await client.PutAsJsonAsync(uri, value);
        return await ReadSuccessJsonAsync(response);
    }

    private static async Task<JsonElement> ReadSuccessJsonAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);
        return JsonDocument.Parse(body).RootElement.Clone();
    }

    private static void AssertTeachingNoteHasNoLegacyFields(JsonElement note)
    {
        Assert.False(note.TryGetProperty("title", out _));
        Assert.False(note.TryGetProperty("status", out _));
        Assert.False(note.TryGetProperty("nextAction", out _));
        Assert.False(note.TryGetProperty("sortOrder", out _));
        Assert.False(note.TryGetProperty("targetType", out _));
        Assert.False(note.TryGetProperty("targetId", out _));
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

    private static void CreateStyledQuestionDocx(string docxPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);

        var document = new Document();
        var body = document.FirstSection.Body;
        body.RemoveAllChildren();

        AddStyledParagraph(document, body, "例题", "题干第一段");
        AddStyledParagraph(document, body, "答案", "答案第一段");
        AddStyledParagraph(document, body, "解析", "解析第一段");

        document.Save(docxPath);
    }

    private static void CreateMultiQuestionImportDocx(string docxPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);

        var document = new Document();
        var body = document.FirstSection.Body;
        body.RemoveAllChildren();

        AddStyledParagraph(document, body, "正文", "导入前说明，应该被丢弃。");
        AddStyledParagraph(document, body, "例题", "第一题题干");
        AddStyledParagraph(document, body, "答案", "第一题答案");
        AddStyledParagraph(document, body, "典型例题", "第二题题干");
        AddStyledParagraph(document, body, "答案", "第二题答案");
        AddStyledParagraph(document, body, "练习题", "第三题题干");
        AddStyledParagraph(document, body, "答案", "第三题答案");

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

        var style = document.Styles.Add(StyleType.Paragraph, styleName);
        style.Font.Name = "宋体";
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
        private readonly bool _useLegacyConfiguration;

        public CmsV2ApiFactory(bool useLegacyConfiguration = false)
        {
            _useLegacyConfiguration = useLegacyConfiguration;
        }

        public string BankRootDirectory { get; } = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-api-tests",
            Guid.NewGuid().ToString("N"));

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.Sources.Clear();
                configuration.AddInMemoryCollection(_useLegacyConfiguration
                    ? CreateLegacyConfiguration()
                    : CreateThreeBankConfiguration());
            });
        }

        private Dictionary<string, string?> CreateLegacyConfiguration()
        {
            return new Dictionary<string, string?>
            {
                ["CmsV2:BankRootDirectory"] = BankRootDirectory
            };
        }

        private Dictionary<string, string?> CreateThreeBankConfiguration()
        {
            return new Dictionary<string, string?>
            {
                ["CmsV2:ActiveBankKey"] = "test",
                ["CmsV2:Banks:0:Key"] = "TEST",
                ["CmsV2:Banks:0:DisplayName"] = "测试题库",
                ["CmsV2:Banks:0:Kind"] = "Test",
                ["CmsV2:Banks:0:RootDirectory"] = BankRootDirectory,
                ["CmsV2:Banks:1:Key"] = "GZ",
                ["CmsV2:Banks:1:DisplayName"] = "高中题库",
                ["CmsV2:Banks:1:Kind"] = "Production",
                ["CmsV2:Banks:1:RootDirectory"] = Path.Combine(Path.GetDirectoryName(BankRootDirectory)!, "gz-bank"),
                ["CmsV2:Banks:2:Key"] = "CZ",
                ["CmsV2:Banks:2:DisplayName"] = "初中题库",
                ["CmsV2:Banks:2:Kind"] = "Production",
                ["CmsV2:Banks:2:RootDirectory"] = Path.Combine(Path.GetDirectoryName(BankRootDirectory)!, "cz-bank")
            };
        }
    }
}
