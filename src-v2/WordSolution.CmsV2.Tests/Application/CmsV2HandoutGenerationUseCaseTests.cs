using System.IO.Compression;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Application.Handouts;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Infrastructure.Documents;
using WordSolution.CmsV2.Infrastructure.Persistence;
using WordSolution.CmsV2.Infrastructure.Repositories;
using AsposeDocumentBuilder = Aspose.Words.DocumentBuilder;
using AsposeNodeType = Aspose.Words.NodeType;
using AsposeDocument = Aspose.Words.Document;
using AsposeParagraph = Aspose.Words.Paragraph;
using AsposeRun = Aspose.Words.Run;
using AsposeSaveFormat = Aspose.Words.SaveFormat;
using AsposeStyleType = Aspose.Words.StyleType;

namespace WordSolution.CmsV2.Tests.Application;

public sealed class CmsV2HandoutGenerationUseCaseTests
{
    private const string LegacyDefaultOutputTemplateDocxPath =
        "src-v2/WordSolution.CmsV2.Infrastructure/Documents/Templates/content-block-default.docx";

    [Fact]
    public async Task GenerateHandoutWord_generates_file_generated_record_and_manifest_for_direct_content_block()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var generatedTime = new DateTimeOffset(2026, 6, 9, 0, 0, 0, TimeSpan.Zero);
        var setup = await CreateOutputFormAsync(unitOfWork, bankRootDirectory);
        var block = await CreateContentBlockWithVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "动能定理",
            "合外力做功等于动能变化",
            versionNumber: 1,
            isCurrent: true);
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.ContentBlock,
            block.ContentBlock.Id,
            sortOrder: 1));
        await unitOfWork.SaveChangesAsync();
        var useCases = CreateUseCases(unitOfWork);

        var result = await useCases.GenerateHandoutWordAsync(
            new GenerateHandoutWordCommand(bankRootDirectory, setup.OutputForm.Id, generatedTime));

        var generatedFiles = await unitOfWork.GeneratedFiles.ListByOutputFormAsync(setup.OutputForm.Id);
        var outputText = ReadDocxText(result.FilePath);
        using var manifest = JsonDocument.Parse(result.VersionManifestJson);

        Assert.True(File.Exists(result.FilePath));
        Assert.Single(generatedFiles);
        Assert.Equal(generatedFiles.Single().Id, result.GeneratedFileId);
        Assert.Equal(result.FilePath, generatedFiles.Single().FilePath);
        Assert.Contains("合外力做功等于动能变化", outputText);
        Assert.Equal(1, manifest.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(setup.OutputForm.Id, manifest.RootElement.GetProperty("outputFormId").GetInt32());
        Assert.Equal(setup.HandoutVersion.Id, manifest.RootElement.GetProperty("handoutVersionId").GetInt32());
        var source = manifest.RootElement.GetProperty("sources")[0];
        Assert.Equal(1, source.GetProperty("sequence").GetInt32());
        Assert.Equal(block.ContentBlock.Id, source.GetProperty("contentBlockId").GetInt32());
        Assert.Equal(block.Version.Id, source.GetProperty("contentBlockVersionId").GetInt32());
        Assert.Equal(1, source.GetProperty("versionNumber").GetInt32());
        Assert.Equal(block.Version.DocxPath, source.GetProperty("docxPath").GetString());
    }

    [Fact]
    public async Task GenerateHandoutWord_resolves_legacy_default_template_path_from_runtime_output_directory()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var generatedTime = new DateTimeOffset(2026, 6, 24, 0, 0, 0, TimeSpan.Zero);
        var setup = await CreateOutputFormAsync(
            unitOfWork,
            bankRootDirectory,
            templateDocxPath: LegacyDefaultOutputTemplateDocxPath);
        await CreateRuntimeDefaultOutputTemplateAsync("Runtime default template");
        var block = await CreateContentBlockWithVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "默认模板路径兼容",
            "旧默认模板路径可生成正文",
            versionNumber: 1,
            isCurrent: true);
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.ContentBlock,
            block.ContentBlock.Id,
            sortOrder: 1));
        await unitOfWork.SaveChangesAsync();

        var result = await CreateUseCases(unitOfWork).GenerateHandoutWordAsync(
            new GenerateHandoutWordCommand(bankRootDirectory, setup.OutputForm.Id, generatedTime));
        var outputText = ReadDocxText(result.FilePath);

        Assert.True(File.Exists(result.FilePath));
        Assert.Contains("旧默认模板路径可生成正文", outputText);
    }

    [Fact]
    public async Task GenerateHandoutWord_appends_untitled_content_block_without_extra_title()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var setup = await CreateOutputFormAsync(unitOfWork, bankRootDirectory);
        var block = await CreateContentBlockWithVersionAsync(
            unitOfWork,
            bankRootDirectory,
            string.Empty,
            "Untitled content block body",
            versionNumber: 1,
            isCurrent: true);
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.ContentBlock,
            block.ContentBlock.Id,
            sortOrder: 1));
        await unitOfWork.SaveChangesAsync();

        var result = await CreateUseCases(unitOfWork).GenerateHandoutWordAsync(
            new GenerateHandoutWordCommand(bankRootDirectory, setup.OutputForm.Id));
        var outputText = ReadDocxText(result.FilePath);

        Assert.True(File.Exists(result.FilePath));
        Assert.Contains("Untitled content block body", outputText);
    }

    [Fact]
    public async Task GenerateHandoutWord_expands_section_variant_and_uses_locked_section_item_version()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var setup = await CreateOutputFormAsync(unitOfWork, bankRootDirectory);
        var versionedBlock = await CreateContentBlockWithTwoVersionsAsync(
            unitOfWork,
            bankRootDirectory,
            "机械能守恒",
            oldText: "锁定旧版本正文",
            currentText: "当前版本正文");
        var topic = new TeachingTopic("机械能");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();
        var section = new Section(topic.Id, "机械能专题");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();
        var sectionItem = new SectionItem(
            section.Id,
            SectionItemTargetType.ContentBlock,
            versionedBlock.ContentBlock.Id,
            ReferenceMode.LockedVersion,
            versionedBlock.OldVersion.Id,
            sortOrder: 1);
        await unitOfWork.SectionItems.AddAsync(sectionItem);
        await unitOfWork.SaveChangesAsync();
        var variant = new SectionVariant(section.Id, "基础讲解版");
        await unitOfWork.SectionVariants.AddAsync(variant);
        await unitOfWork.SaveChangesAsync();
        await unitOfWork.SectionVariantItems.AddAsync(new SectionVariantItem(variant.Id, sectionItem.Id, sortOrder: 1));
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.SectionVariant,
            variant.Id,
            sortOrder: 1));
        await unitOfWork.SaveChangesAsync();

        var result = await CreateUseCases(unitOfWork).GenerateHandoutWordAsync(
            new GenerateHandoutWordCommand(bankRootDirectory, setup.OutputForm.Id));
        var outputText = ReadDocxText(result.FilePath);
        using var manifest = JsonDocument.Parse(result.VersionManifestJson);
        var source = manifest.RootElement.GetProperty("sources")[0];

        Assert.Contains("锁定旧版本正文", outputText);
        Assert.DoesNotContain("当前版本正文", outputText);
        Assert.Equal(versionedBlock.OldVersion.Id, source.GetProperty("contentBlockVersionId").GetInt32());
        Assert.Equal(1, source.GetProperty("versionNumber").GetInt32());
    }

    [Fact]
    public async Task GenerateHandoutWord_expands_atomic_sections_and_content_block_relations()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var setup = await CreateOutputFormAsync(unitOfWork, bankRootDirectory);
        var atomicBlock = await CreateContentBlockWithVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "原子讲解",
            "原子小节正文",
            versionNumber: 1,
            isCurrent: true);
        var parentBlock = await CreateContentBlockWithVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "组合导语",
            "组合块导语正文",
            versionNumber: 1,
            isCurrent: true);
        var childBlock = await CreateContentBlockWithVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "组合子块",
            "组合子块正文",
            versionNumber: 1,
            isCurrent: true);
        await unitOfWork.ContentBlockRelations.AddAsync(new ContentBlockRelation(
            parentBlock.ContentBlock.Id,
            childBlock.ContentBlock.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 1));
        var topic = new TeachingTopic("圆周运动");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();
        var section = new Section(topic.Id, "竖直圆轨道");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();

        var atomicSection = new AtomicSection(section.Id, "圆轨道条件");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();
        var atomicSectionItem = new AtomicSectionItem(
            atomicSection.Id,
            atomicBlock.ContentBlock.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 1);
        await unitOfWork.AtomicSectionItems.AddAsync(atomicSectionItem);
        var sectionItem = new SectionItem(
            section.Id,
            SectionItemTargetType.AtomicSection,
            atomicSection.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 1);
        await unitOfWork.SectionItems.AddAsync(sectionItem);
        await unitOfWork.SaveChangesAsync();
        var variant = new SectionVariant(section.Id, "课堂版");
        await unitOfWork.SectionVariants.AddAsync(variant);
        await unitOfWork.SaveChangesAsync();
        await unitOfWork.SectionVariantItems.AddAsync(new SectionVariantItem(variant.Id, sectionItem.Id, sortOrder: 1));
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.SectionVariant,
            variant.Id,
            sortOrder: 1));
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.ContentBlock,
            parentBlock.ContentBlock.Id,
            sortOrder: 2));
        await unitOfWork.SaveChangesAsync();

        var result = await CreateUseCases(unitOfWork).GenerateHandoutWordAsync(
            new GenerateHandoutWordCommand(bankRootDirectory, setup.OutputForm.Id));
        var outputText = ReadDocxText(result.FilePath);
        using var manifest = JsonDocument.Parse(result.VersionManifestJson);
        var sources = manifest.RootElement.GetProperty("sources").EnumerateArray().ToList();

        Assert.True(outputText.IndexOf("原子小节正文", StringComparison.Ordinal) < outputText.IndexOf("组合块导语正文", StringComparison.Ordinal));
        Assert.True(outputText.IndexOf("组合块导语正文", StringComparison.Ordinal) < outputText.IndexOf("组合子块正文", StringComparison.Ordinal));
        Assert.Equal(
            [atomicBlock.ContentBlock.Id, parentBlock.ContentBlock.Id, childBlock.ContentBlock.Id],
            sources.Select(source => source.GetProperty("contentBlockId").GetInt32()).ToArray());
        AssertNoInsertedSectionBreaks(result.FilePath);
    }

    [Fact]
    public async Task GenerateHandoutWord_expands_direct_atomic_section_items()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var setup = await CreateOutputFormAsync(unitOfWork, bankRootDirectory);
        var block = await CreateContentBlockWithVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "Atomic child",
            "Direct AtomicSection content",
            versionNumber: 1,
            isCurrent: true);
        var topic = new TeachingTopic("Direct Atomic Topic");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();
        var section = new Section(topic.Id, "Direct Atomic Section");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();
        var atomicSection = new AtomicSection(section.Id, "Direct AtomicSection");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();
        await unitOfWork.AtomicSectionItems.AddAsync(new AtomicSectionItem(
            atomicSection.Id,
            block.ContentBlock.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 1));
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.AtomicSection,
            atomicSection.Id,
            sortOrder: 1));
        await unitOfWork.SaveChangesAsync();

        var result = await CreateUseCases(unitOfWork).GenerateHandoutWordAsync(
            new GenerateHandoutWordCommand(bankRootDirectory, setup.OutputForm.Id));
        var outputText = ReadDocxText(result.FilePath);
        using var manifest = JsonDocument.Parse(result.VersionManifestJson);
        var source = manifest.RootElement.GetProperty("sources")[0];

        Assert.Contains("Direct AtomicSection", outputText);
        Assert.Contains("Direct AtomicSection content", outputText);
        Assert.Equal(block.ContentBlock.Id, source.GetProperty("contentBlockId").GetInt32());
        Assert.Equal(block.Version.Id, source.GetProperty("contentBlockVersionId").GetInt32());
    }

    [Fact]
    public async Task GenerateHandoutWord_expands_atomic_section_panels_before_unassigned_items_without_panel_titles()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var setup = await CreateOutputFormAsync(unitOfWork, bankRootDirectory);
        var unassigned = await CreateContentBlockWithVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "Unassigned",
            "Unassigned content",
            versionNumber: 1,
            isCurrent: true);
        var example = await CreateContentBlockWithVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "Example",
            "Example panel content",
            versionNumber: 1,
            isCurrent: true);
        var practice = await CreateContentBlockWithVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "Practice",
            "Practice panel content",
            versionNumber: 1,
            isCurrent: true);
        var topic = new TeachingTopic("Panel Topic");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();
        var section = new Section(topic.Id, "Panel Section");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();
        var atomicSection = new AtomicSection(section.Id, "Panel AtomicSection");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();
        var examplePanel = new AtomicSectionPanel(
            atomicSection.Id,
            "Example Panel Title",
            AtomicSectionTeachingRole.Example,
            Difficulty.Basic,
            sortOrder: 10);
        var practicePanel = new AtomicSectionPanel(
            atomicSection.Id,
            "Practice Panel Title",
            AtomicSectionTeachingRole.Practice,
            Difficulty.Basic,
            sortOrder: 20);
        await unitOfWork.AtomicSectionPanels.AddAsync(examplePanel);
        await unitOfWork.AtomicSectionPanels.AddAsync(practicePanel);
        await unitOfWork.SaveChangesAsync();
        await unitOfWork.AtomicSectionItems.AddAsync(new AtomicSectionItem(
            atomicSection.Id,
            unassigned.ContentBlock.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 1));
        await unitOfWork.AtomicSectionItems.AddAsync(new AtomicSectionItem(
            atomicSection.Id,
            example.ContentBlock.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 10,
            atomicSectionPanelId: examplePanel.Id,
            teachingRole: AtomicSectionTeachingRole.Example));
        await unitOfWork.AtomicSectionItems.AddAsync(new AtomicSectionItem(
            atomicSection.Id,
            practice.ContentBlock.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 10,
            atomicSectionPanelId: practicePanel.Id,
            teachingRole: AtomicSectionTeachingRole.Practice));
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.AtomicSection,
            atomicSection.Id,
            sortOrder: 1));
        await unitOfWork.SaveChangesAsync();

        var result = await CreateUseCases(unitOfWork).GenerateHandoutWordAsync(
            new GenerateHandoutWordCommand(bankRootDirectory, setup.OutputForm.Id));
        var outputText = ReadDocxText(result.FilePath);
        var exampleIndex = outputText.IndexOf("Example panel content", StringComparison.Ordinal);
        var practiceIndex = outputText.IndexOf("Practice panel content", StringComparison.Ordinal);
        var unassignedIndex = outputText.IndexOf("Unassigned content", StringComparison.Ordinal);

        Assert.True(exampleIndex >= 0);
        Assert.True(practiceIndex > exampleIndex);
        Assert.True(unassignedIndex > practiceIndex);
        Assert.DoesNotContain("Example Panel Title", outputText);
        Assert.DoesNotContain("Practice Panel Title", outputText);
    }

    [Fact]
    public async Task GenerateHandoutWord_outputs_teaching_topic_section_and_atomic_section_structure_titles()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var setup = await CreateOutputFormAsync(unitOfWork, bankRootDirectory);
        var block = await CreateContentBlockWithVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "Render child block",
            "Render plan content",
            versionNumber: 1,
            isCurrent: true);
        var topic = new TeachingTopic("Render TeachingTopic");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();
        var section = new Section(topic.Id, "Render Section");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();
        var atomicSection = new AtomicSection(section.Id, "Render AtomicSection");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();
        await unitOfWork.AtomicSectionItems.AddAsync(new AtomicSectionItem(
            atomicSection.Id,
            block.ContentBlock.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 1));
        var sectionItem = new SectionItem(
            section.Id,
            SectionItemTargetType.AtomicSection,
            atomicSection.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 1);
        await unitOfWork.SectionItems.AddAsync(sectionItem);
        await unitOfWork.SaveChangesAsync();
        var variant = new SectionVariant(section.Id, "Render SectionVariant");
        await unitOfWork.SectionVariants.AddAsync(variant);
        await unitOfWork.SaveChangesAsync();
        await unitOfWork.SectionVariantItems.AddAsync(new SectionVariantItem(
            variant.Id,
            sectionItem.Id,
            sortOrder: 1));
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.SectionVariant,
            variant.Id,
            sortOrder: 1));
        await unitOfWork.SaveChangesAsync();

        var result = await CreateUseCases(unitOfWork).GenerateHandoutWordAsync(
            new GenerateHandoutWordCommand(bankRootDirectory, setup.OutputForm.Id));
        var outputText = ReadDocxText(result.FilePath);

        var topicIndex = outputText.IndexOf("Render TeachingTopic", StringComparison.Ordinal);
        var sectionIndex = outputText.IndexOf("Render Section", StringComparison.Ordinal);
        var atomicIndex = outputText.IndexOf("Render AtomicSection", StringComparison.Ordinal);
        var contentIndex = outputText.IndexOf("Render plan content", StringComparison.Ordinal);

        Assert.True(topicIndex >= 0);
        Assert.True(sectionIndex > topicIndex);
        Assert.True(atomicIndex > sectionIndex);
        Assert.True(contentIndex > atomicIndex);
        AssertHeadingUsesTemplateStyleWithoutDirectRunFormatting(result.FilePath, "Render TeachingTopic", "Heading1");
        AssertHeadingUsesTemplateStyleWithoutDirectRunFormatting(result.FilePath, "Render Section", "Heading2");
        AssertHeadingUsesTemplateStyleWithoutDirectRunFormatting(result.FilePath, "Render AtomicSection", "Heading3");
    }

    [Fact]
    public async Task GenerateHandoutWord_rebinds_question_stem_style_by_atomic_section_occurrence()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var setup = await CreateOutputFormAsync(unitOfWork, bankRootDirectory);
        CreateTemplateWithQuestionOutputStyles(
            setup.OutputTemplate.TemplateDocxPath,
            "例题",
            "变式",
            "练习题");
        var exampleBlock = await CreateQuestionContentBlockWithStyledVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "Example question",
            "Example stem");
        var variantBlock = await CreateQuestionContentBlockWithStyledVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "Variant question",
            "Variant stem");
        var practiceBlock = await CreateQuestionContentBlockWithStyledVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "Practice question",
            "Practice stem");
        var topic = new TeachingTopic("Style Topic");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();
        var section = new Section(topic.Id, "Style Section");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();
        var atomicSection = new AtomicSection(section.Id, "Style AtomicSection");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();
        var examplePanel = new AtomicSectionPanel(
            atomicSection.Id,
            "Example panel",
            AtomicSectionTeachingRole.Example,
            Difficulty.Basic,
            sortOrder: 1);
        var variantPanel = new AtomicSectionPanel(
            atomicSection.Id,
            "Variant panel",
            AtomicSectionTeachingRole.Variant,
            Difficulty.Medium,
            sortOrder: 2);
        var practicePanel = new AtomicSectionPanel(
            atomicSection.Id,
            "Practice panel",
            AtomicSectionTeachingRole.Practice,
            Difficulty.Medium,
            sortOrder: 3);
        await unitOfWork.AtomicSectionPanels.AddAsync(examplePanel);
        await unitOfWork.AtomicSectionPanels.AddAsync(variantPanel);
        await unitOfWork.AtomicSectionPanels.AddAsync(practicePanel);
        await unitOfWork.SaveChangesAsync();
        await unitOfWork.AtomicSectionItems.AddAsync(new AtomicSectionItem(
            atomicSection.Id,
            exampleBlock.ContentBlock.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 1,
            atomicSectionPanelId: examplePanel.Id));
        await unitOfWork.AtomicSectionItems.AddAsync(new AtomicSectionItem(
            atomicSection.Id,
            variantBlock.ContentBlock.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 1,
            atomicSectionPanelId: variantPanel.Id));
        await unitOfWork.AtomicSectionItems.AddAsync(new AtomicSectionItem(
            atomicSection.Id,
            practiceBlock.ContentBlock.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 1,
            atomicSectionPanelId: practicePanel.Id));
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.AtomicSection,
            atomicSection.Id,
            sortOrder: 1));
        await unitOfWork.SaveChangesAsync();

        var result = await CreateUseCases(unitOfWork).GenerateHandoutWordAsync(
            new GenerateHandoutWordCommand(bankRootDirectory, setup.OutputForm.Id));

        AssertParagraphUsesStyleName(result.FilePath, "Example stem", "例题");
        AssertParagraphUsesStyleName(result.FilePath, "Variant stem", "变式");
        AssertParagraphUsesStyleName(result.FilePath, "Practice stem", "练习题");
        AssertNoInsertedSectionBreaks(result.FilePath);
    }

    [Fact]
    public async Task GenerateHandoutWord_skips_question_without_effective_stem_and_keeps_generation_valid()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var setup = await CreateOutputFormAsync(unitOfWork, bankRootDirectory);
        CreateTemplateWithQuestionOutputStyles(
            setup.OutputTemplate.TemplateDocxPath,
            "例题",
            "变式",
            "练习题");
        var validBlock = await CreateQuestionContentBlockWithStyledVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "Valid question",
            "Valid stem");
        var skippedBlock = await CreateQuestionContentBlockWithOnlyOtherVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "Only other question",
            "Unknown style content");
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.ContentBlock,
            validBlock.ContentBlock.Id,
            sortOrder: 1));
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.ContentBlock,
            skippedBlock.ContentBlock.Id,
            sortOrder: 2));
        await unitOfWork.SaveChangesAsync();
        var useCases = CreateUseCases(unitOfWork);

        var validation = await useCases.ValidateHandoutWordGenerationAsync(
            new ValidateHandoutWordGenerationCommand(bankRootDirectory, setup.OutputForm.Id));
        var result = await useCases.GenerateHandoutWordAsync(
            new GenerateHandoutWordCommand(bankRootDirectory, setup.OutputForm.Id));
        var outputText = ReadDocxText(result.FilePath);
        using var manifest = JsonDocument.Parse(result.VersionManifestJson);
        var generatedFiles = await unitOfWork.GeneratedFiles.ListByOutputFormAsync(setup.OutputForm.Id);
        var source = Assert.Single(manifest.RootElement.GetProperty("sources").EnumerateArray());

        var issue = Assert.Single(validation.Issues);
        Assert.True(validation.IsValid);
        Assert.Equal("MissingQuestionStem", issue.Code);
        Assert.Equal(setup.OutputForm.Id, issue.OutputFormId);
        Assert.Equal(skippedBlock.ContentBlock.Id, issue.ContentBlockId);
        Assert.Equal(skippedBlock.Version.Id, issue.ContentBlockVersionId);
        Assert.Equal("练习题", issue.RequiredStyleName);
        Assert.Equal("Practice", issue.OccurrenceRole);
        Assert.Contains("Valid stem", outputText);
        Assert.DoesNotContain("Unknown style content", outputText);
        Assert.Single(generatedFiles);
        Assert.Equal(validBlock.ContentBlock.Id, source.GetProperty("contentBlockId").GetInt32());
        Assert.Equal(validBlock.Version.Id, source.GetProperty("contentBlockVersionId").GetInt32());
        Assert.Equal(1, source.GetProperty("sequence").GetInt32());
    }

    [Fact]
    public async Task GenerateHandoutWord_rebinds_first_effective_stem_only_and_does_not_use_other_as_fallback()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var setup = await CreateOutputFormAsync(unitOfWork, bankRootDirectory);
        CreateTemplateWithQuestionOutputStyles(
            setup.OutputTemplate.TemplateDocxPath,
            "例题",
            "变式",
            "练习题");
        var block = await CreateQuestionContentBlockWithOtherBeforeStemAsync(
            unitOfWork,
            bankRootDirectory,
            "Other before stem",
            "Other lead",
            "Actual stem");
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.ContentBlock,
            block.ContentBlock.Id,
            sortOrder: 1));
        await unitOfWork.SaveChangesAsync();

        var result = await CreateUseCases(unitOfWork).GenerateHandoutWordAsync(
            new GenerateHandoutWordCommand(bankRootDirectory, setup.OutputForm.Id));

        AssertParagraphUsesStyleName(result.FilePath, "Other lead", "未知样式");
        AssertParagraphUsesStyleName(result.FilePath, "Actual stem", "练习题");
    }

    [Fact]
    public async Task ValidateHandoutWordGeneration_allows_non_question_content_without_stem()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var setup = await CreateOutputFormAsync(unitOfWork, bankRootDirectory);
        var block = await CreateContentBlockWithVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "普通文本",
            "No question stem here",
            versionNumber: 1,
            isCurrent: true,
            blockType: ContentBlockType.GeneralText);
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.ContentBlock,
            block.ContentBlock.Id,
            sortOrder: 1));
        await unitOfWork.SaveChangesAsync();

        var validation = await CreateUseCases(unitOfWork).ValidateHandoutWordGenerationAsync(
            new ValidateHandoutWordGenerationCommand(bankRootDirectory, setup.OutputForm.Id));

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Issues);
    }

    [Fact]
    public async Task GenerateHandoutWord_rejects_missing_question_output_style_without_generated_record()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var setup = await CreateOutputFormAsync(unitOfWork, bankRootDirectory);
        CreateTemplateWithQuestionOutputStyles(
            setup.OutputTemplate.TemplateDocxPath,
            "例题",
            "练习题");
        var variantBlock = await CreateQuestionContentBlockWithStyledVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "Variant question",
            "Variant stem");
        var topic = new TeachingTopic("Missing Style Topic");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();
        var section = new Section(topic.Id, "Missing Style Section");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();
        var atomicSection = new AtomicSection(section.Id, "Missing Style AtomicSection");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();
        await unitOfWork.AtomicSectionItems.AddAsync(new AtomicSectionItem(
            atomicSection.Id,
            variantBlock.ContentBlock.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 1,
            teachingRole: AtomicSectionTeachingRole.Variant));
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.AtomicSection,
            atomicSection.Id,
            sortOrder: 1));
        await unitOfWork.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => CreateUseCases(unitOfWork).GenerateHandoutWordAsync(
                new GenerateHandoutWordCommand(bankRootDirectory, setup.OutputForm.Id)));

        Assert.Contains("变式", exception.Message);
        Assert.Empty(await unitOfWork.GeneratedFiles.ListByOutputFormAsync(setup.OutputForm.Id));
    }

    [Fact]
    public async Task ValidateHandoutWordGeneration_reports_missing_output_style_without_generated_record()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var setup = await CreateOutputFormAsync(unitOfWork, bankRootDirectory);
        CreateTemplateWithQuestionOutputStyles(
            setup.OutputTemplate.TemplateDocxPath,
            "例题",
            "练习题");
        var variantBlock = await CreateQuestionContentBlockWithStyledVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "Variant question",
            "Variant stem");
        var topic = new TeachingTopic("Validate Missing Style Topic");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();
        var section = new Section(topic.Id, "Validate Missing Style Section");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();
        var atomicSection = new AtomicSection(section.Id, "Validate Missing Style AtomicSection");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();
        await unitOfWork.AtomicSectionItems.AddAsync(new AtomicSectionItem(
            atomicSection.Id,
            variantBlock.ContentBlock.Id,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 1,
            teachingRole: AtomicSectionTeachingRole.Variant));
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.AtomicSection,
            atomicSection.Id,
            sortOrder: 1));
        await unitOfWork.SaveChangesAsync();

        var validation = await CreateUseCases(unitOfWork).ValidateHandoutWordGenerationAsync(
            new ValidateHandoutWordGenerationCommand(bankRootDirectory, setup.OutputForm.Id));

        var issue = Assert.Single(validation.Issues);
        Assert.False(validation.IsValid);
        Assert.Equal("MissingOutputStyle", issue.Code);
        Assert.Equal(setup.OutputForm.Id, issue.OutputFormId);
        Assert.Equal(setup.OutputTemplate.Id, issue.OutputTemplateId);
        Assert.Equal("变式", issue.RequiredStyleName);
        Assert.Empty(await unitOfWork.GeneratedFiles.ListByOutputFormAsync(setup.OutputForm.Id));
    }

    [Theory]
    [InlineData(OutputFormat.Pdf)]
    [InlineData(OutputFormat.WordAndPdf)]
    public async Task GenerateHandoutWord_rejects_unsupported_output_formats_without_generated_record(OutputFormat outputFormat)
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var setup = await CreateOutputFormAsync(unitOfWork, bankRootDirectory, outputFormat);

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => CreateUseCases(unitOfWork).GenerateHandoutWordAsync(
                new GenerateHandoutWordCommand(bankRootDirectory, setup.OutputForm.Id)));

        Assert.Empty(await unitOfWork.GeneratedFiles.ListByOutputFormAsync(setup.OutputForm.Id));
    }

    [Fact]
    public async Task GenerateHandoutWord_rejects_missing_template_or_source_file_without_generated_record()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var setup = await CreateOutputFormAsync(unitOfWork, bankRootDirectory);
        var block = await CreateContentBlockWithVersionAsync(
            unitOfWork,
            bankRootDirectory,
            "缺文件内容块",
            "正文",
            versionNumber: 1,
            isCurrent: true);
        File.Delete(block.Version.DocxPath);
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.ContentBlock,
            block.ContentBlock.Id,
            sortOrder: 1));
        await unitOfWork.SaveChangesAsync();

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => CreateUseCases(unitOfWork).GenerateHandoutWordAsync(
                new GenerateHandoutWordCommand(bankRootDirectory, setup.OutputForm.Id)));

        Assert.Empty(await unitOfWork.GeneratedFiles.ListByOutputFormAsync(setup.OutputForm.Id));
    }

    [Fact]
    public async Task GenerateHandoutWord_rejects_recursive_content_block_relations()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var bankRootDirectory = CreateTempRoot();
        var setup = await CreateOutputFormAsync(unitOfWork, bankRootDirectory);
        var first = await CreateContentBlockWithVersionAsync(unitOfWork, bankRootDirectory, "A", "A 正文", 1, true);
        var second = await CreateContentBlockWithVersionAsync(unitOfWork, bankRootDirectory, "B", "B 正文", 1, true);
        await unitOfWork.ContentBlockRelations.AddAsync(new ContentBlockRelation(
            first.ContentBlock.Id,
            second.ContentBlock.Id,
            ReferenceMode.FollowLatest,
            null,
            sortOrder: 1));
        await unitOfWork.ContentBlockRelations.AddAsync(new ContentBlockRelation(
            second.ContentBlock.Id,
            first.ContentBlock.Id,
            ReferenceMode.FollowLatest,
            null,
            sortOrder: 1));
        await unitOfWork.HandoutVersionItems.AddAsync(new HandoutVersionItem(
            setup.HandoutVersion.Id,
            HandoutVersionItemTargetType.ContentBlock,
            first.ContentBlock.Id,
            sortOrder: 1));
        await unitOfWork.SaveChangesAsync();

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => CreateUseCases(unitOfWork).GenerateHandoutWordAsync(
                new GenerateHandoutWordCommand(bankRootDirectory, setup.OutputForm.Id)));

        Assert.Empty(await unitOfWork.GeneratedFiles.ListByOutputFormAsync(setup.OutputForm.Id));
    }

    private static HandoutGenerationUseCases CreateUseCases(EfCmsV2UnitOfWork unitOfWork)
    {
        return new HandoutGenerationUseCases(
            unitOfWork,
            new CmsV2FileAssetPathProvider(),
            new LocalContentBlockFileStore(),
            new OutputTemplatePathResolver(
                AppContext.BaseDirectory,
                Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))),
            new AsposeHandoutDocumentGenerator());
    }

    private static async Task<OutputSetup> CreateOutputFormAsync(
        EfCmsV2UnitOfWork unitOfWork,
        string bankRootDirectory,
        OutputFormat outputFormat = OutputFormat.Word,
        string? templateDocxPath = null)
    {
        var templatePath = templateDocxPath ?? Path.Combine(bankRootDirectory, "templates", "default.docx");
        if (templateDocxPath is null)
        {
            await CreateMinimalDocxAsync(templatePath, "模板正文");
        }
        var handout = new Handout("机械能讲义");
        await unitOfWork.Handouts.AddAsync(handout);
        await unitOfWork.SaveChangesAsync();
        var handoutVersion = new HandoutVersion(handout.Id, "基础班");
        await unitOfWork.HandoutVersions.AddAsync(handoutVersion);
        var template = new OutputTemplate("默认模板", templatePath);
        await unitOfWork.OutputTemplates.AddAsync(template);
        await unitOfWork.SaveChangesAsync();
        var outputForm = new OutputForm(
            handoutVersion.Id,
            template.Id,
            "学生版",
            OutputAudience.Student,
            outputFormat,
            VisibilityMode.StudentNoAnswer);
        await unitOfWork.OutputForms.AddAsync(outputForm);
        await unitOfWork.SaveChangesAsync();

        return new OutputSetup(handout, handoutVersion, template, outputForm);
    }

    private static async Task CreateRuntimeDefaultOutputTemplateAsync(string text)
    {
        var runtimeTemplatePath = Path.Combine(
            AppContext.BaseDirectory,
            "Documents",
            "Templates",
            "content-block-default.docx");
        if (File.Exists(runtimeTemplatePath))
        {
            return;
        }

        await CreateMinimalDocxAsync(runtimeTemplatePath, text);
    }

    private static async Task<ContentBlockVersionSetup> CreateContentBlockWithTwoVersionsAsync(
        EfCmsV2UnitOfWork unitOfWork,
        string bankRootDirectory,
        string title,
        string oldText,
        string currentText)
    {
        var oldVersion = await CreateContentBlockWithVersionAsync(
            unitOfWork,
            bankRootDirectory,
            title,
            oldText,
            versionNumber: 1,
            isCurrent: false);
        var currentDocxPath = Path.Combine(
            bankRootDirectory,
            "content-blocks",
            "source",
            oldVersion.ContentBlock.Id.ToString(),
            "v2.docx");
        await CreateMinimalDocxAsync(currentDocxPath, currentText);
        var currentVersion = new ContentBlockVersion(
            oldVersion.ContentBlock.Id,
            versionNumber: 2,
            currentDocxPath,
            plainText: currentText,
            isCurrent: true);
        await unitOfWork.ContentBlockVersions.AddAsync(currentVersion);
        await unitOfWork.SaveChangesAsync();
        oldVersion.ContentBlock.SetCurrentVersion(currentVersion.Id);
        currentVersion.MarkCurrent();
        unitOfWork.ContentBlocks.Update(oldVersion.ContentBlock);
        unitOfWork.ContentBlockVersions.Update(currentVersion);
        await unitOfWork.SaveChangesAsync();

        return new ContentBlockVersionSetup(oldVersion.ContentBlock, oldVersion.Version, currentVersion);
    }

    private static async Task<ContentBlockSetup> CreateContentBlockWithVersionAsync(
        EfCmsV2UnitOfWork unitOfWork,
        string bankRootDirectory,
        string title,
        string text,
        int versionNumber,
        bool isCurrent,
        ContentBlockType blockType = ContentBlockType.GeneralText)
    {
        var sectionId = await CreateSectionAsync(unitOfWork);
        var block = new ContentBlock(sectionId, title, blockType);
        await unitOfWork.ContentBlocks.AddAsync(block);
        await unitOfWork.SaveChangesAsync();
        var docxPath = Path.Combine(
            bankRootDirectory,
            "content-blocks",
            "source",
            block.Id.ToString(),
            $"v{versionNumber}.docx");
        await CreateMinimalDocxAsync(docxPath, text);
        var version = new ContentBlockVersion(
            block.Id,
            versionNumber,
            docxPath,
            plainText: text,
            isCurrent: isCurrent);
        await unitOfWork.ContentBlockVersions.AddAsync(version);
        await unitOfWork.SaveChangesAsync();

        if (isCurrent)
        {
            block.SetCurrentVersion(version.Id);
            version.MarkCurrent();
            unitOfWork.ContentBlocks.Update(block);
            unitOfWork.ContentBlockVersions.Update(version);
            await unitOfWork.SaveChangesAsync();
        }

        return new ContentBlockSetup(block, version);
    }

    private static async Task<ContentBlockSetup> CreateQuestionContentBlockWithStyledVersionAsync(
        EfCmsV2UnitOfWork unitOfWork,
        string bankRootDirectory,
        string title,
        string stemText)
    {
        var sectionId = await CreateSectionAsync(unitOfWork);
        var block = new ContentBlock(sectionId, title, ContentBlockType.Question);
        await unitOfWork.ContentBlocks.AddAsync(block);
        await unitOfWork.SaveChangesAsync();

        var docxPath = Path.Combine(
            bankRootDirectory,
            "content-blocks",
            "source",
            block.Id.ToString(),
            "v1.docx");
        CreateStyledQuestionDocx(docxPath, stemText);
        var version = new ContentBlockVersion(
            block.Id,
            versionNumber: 1,
            docxPath,
            plainText: stemText,
            isCurrent: true);
        await unitOfWork.ContentBlockVersions.AddAsync(version);
        await unitOfWork.SaveChangesAsync();
        block.SetCurrentVersion(version.Id);
        version.MarkCurrent();
        unitOfWork.ContentBlocks.Update(block);
        unitOfWork.ContentBlockVersions.Update(version);
        await unitOfWork.SaveChangesAsync();

        return new ContentBlockSetup(block, version);
    }

    private static async Task<ContentBlockSetup> CreateQuestionContentBlockWithOnlyOtherVersionAsync(
        EfCmsV2UnitOfWork unitOfWork,
        string bankRootDirectory,
        string title,
        string otherText)
    {
        var sectionId = await CreateSectionAsync(unitOfWork);
        var block = new ContentBlock(sectionId, title, ContentBlockType.Question);
        await unitOfWork.ContentBlocks.AddAsync(block);
        await unitOfWork.SaveChangesAsync();

        var docxPath = Path.Combine(
            bankRootDirectory,
            "content-blocks",
            "source",
            block.Id.ToString(),
            "v1.docx");
        CreateOnlyOtherQuestionDocx(docxPath, otherText);
        var version = new ContentBlockVersion(
            block.Id,
            versionNumber: 1,
            docxPath,
            plainText: otherText,
            isCurrent: true);
        await unitOfWork.ContentBlockVersions.AddAsync(version);
        await unitOfWork.SaveChangesAsync();
        block.SetCurrentVersion(version.Id);
        version.MarkCurrent();
        unitOfWork.ContentBlocks.Update(block);
        unitOfWork.ContentBlockVersions.Update(version);
        await unitOfWork.SaveChangesAsync();

        return new ContentBlockSetup(block, version);
    }

    private static async Task<ContentBlockSetup> CreateQuestionContentBlockWithOtherBeforeStemAsync(
        EfCmsV2UnitOfWork unitOfWork,
        string bankRootDirectory,
        string title,
        string otherText,
        string stemText)
    {
        var sectionId = await CreateSectionAsync(unitOfWork);
        var block = new ContentBlock(sectionId, title, ContentBlockType.Question);
        await unitOfWork.ContentBlocks.AddAsync(block);
        await unitOfWork.SaveChangesAsync();

        var docxPath = Path.Combine(
            bankRootDirectory,
            "content-blocks",
            "source",
            block.Id.ToString(),
            "v1.docx");
        CreateQuestionDocxWithOtherBeforeStem(docxPath, otherText, stemText);
        var version = new ContentBlockVersion(
            block.Id,
            versionNumber: 1,
            docxPath,
            plainText: $"{otherText}\n{stemText}",
            isCurrent: true);
        await unitOfWork.ContentBlockVersions.AddAsync(version);
        await unitOfWork.SaveChangesAsync();
        block.SetCurrentVersion(version.Id);
        version.MarkCurrent();
        unitOfWork.ContentBlocks.Update(block);
        unitOfWork.ContentBlockVersions.Update(version);
        await unitOfWork.SaveChangesAsync();

        return new ContentBlockSetup(block, version);
    }

    private static async Task<CmsV2DbContext> CreateMigratedContextAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-handout-generation-use-case-tests",
            Guid.NewGuid().ToString("N"),
            "cms-v2.db");

        var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        return context;
    }

    private static async Task<int> CreateSectionAsync(EfCmsV2UnitOfWork unitOfWork)
    {
        var topic = new TeachingTopic("默认主题");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        var section = new Section(topic.Id, "默认 Section");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();

        return section.Id;
    }

    private static string CreateTempRoot()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "cms-v2-handout-generation-use-case-tests",
            Guid.NewGuid().ToString("N"));
    }

    private static string ReadDocxText(string docxPath)
    {
        var document = new AsposeDocument(docxPath);
        return document.ToString(AsposeSaveFormat.Text);
    }

    private static void AssertHeadingUsesTemplateStyleWithoutDirectRunFormatting(
        string docxPath,
        string headingText,
        string expectedStyleId)
    {
        XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        using var archive = ZipFile.OpenRead(docxPath);
        var entry = archive.GetEntry("word/document.xml") ?? throw new InvalidOperationException("DOCX document.xml was not found.");
        using var stream = entry.Open();
        var documentXml = XDocument.Load(stream);
        var paragraph = documentXml
            .Descendants(w + "p")
            .Single(paragraph => string.Concat(paragraph.Descendants(w + "t").Select(text => text.Value)) == headingText);

        Assert.Equal(expectedStyleId, paragraph.Descendants(w + "pStyle").Single().Attribute(w + "val")?.Value);
        Assert.Empty(paragraph.Descendants(w + "rPr"));
    }

    private static void AssertNoInsertedSectionBreaks(string docxPath)
    {
        XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        using var archive = ZipFile.OpenRead(docxPath);
        var entry = archive.GetEntry("word/document.xml") ?? throw new InvalidOperationException("DOCX document.xml was not found.");
        using var stream = entry.Open();
        var documentXml = XDocument.Load(stream);

        Assert.Empty(documentXml.Descendants(w + "pPr").Elements(w + "sectPr"));
        Assert.Single(documentXml.Descendants(w + "body").Elements(w + "sectPr"));
    }

    private static void AssertParagraphUsesStyleName(string docxPath, string paragraphText, string expectedStyleName)
    {
        var document = new AsposeDocument(docxPath);
        var paragraph = document
            .GetChildNodes(AsposeNodeType.Paragraph, true)
            .OfType<AsposeParagraph>()
            .Single(paragraph => string.Equals(
                paragraph.GetText().Trim(),
                paragraphText,
                StringComparison.Ordinal));

        Assert.Equal(expectedStyleName, paragraph.ParagraphFormat.StyleName);
    }

    private static void CreateTemplateWithQuestionOutputStyles(string docxPath, params string[] styleNames)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);

        var document = new AsposeDocument();
        var builder = new AsposeDocumentBuilder(document);
        builder.Writeln("Template body");

        foreach (var styleName in styleNames)
        {
            if (document.Styles[styleName] is not null)
            {
                continue;
            }

            var style = document.Styles.Add(AsposeStyleType.Paragraph, styleName);
            style.Font.Name = "Microsoft YaHei";
            style.Font.Size = 12;
            style.Font.Bold = true;
        }

        document.Save(docxPath);
    }

    private static void CreateStyledQuestionDocx(string docxPath, string stemText)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);

        var document = new AsposeDocument();
        var body = document.FirstSection.Body;
        body.RemoveAllChildren();
        AddStyledParagraph(document, body, "正文", stemText);
        AddStyledParagraph(document, body, "答案", $"{stemText} answer");
        document.Save(docxPath);
    }

    private static void CreateOnlyOtherQuestionDocx(string docxPath, string otherText)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);

        var document = new AsposeDocument();
        var body = document.FirstSection.Body;
        body.RemoveAllChildren();
        AddStyledParagraph(document, body, "未知样式", otherText);
        AddStyledParagraph(document, body, "答案", $"{otherText} answer");
        document.Save(docxPath);
    }

    private static void CreateQuestionDocxWithOtherBeforeStem(string docxPath, string otherText, string stemText)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);

        var document = new AsposeDocument();
        var body = document.FirstSection.Body;
        body.RemoveAllChildren();
        AddStyledParagraph(document, body, "未知样式", otherText);
        AddStyledParagraph(document, body, "正文", stemText);
        AddStyledParagraph(document, body, "答案", $"{stemText} answer");
        document.Save(docxPath);
    }

    private static void AddStyledParagraph(AsposeDocument document, Aspose.Words.Body body, string styleName, string text)
    {
        if (document.Styles[styleName] is null)
        {
            document.Styles.Add(AsposeStyleType.Paragraph, styleName);
        }

        var paragraph = new AsposeParagraph(document);
        paragraph.ParagraphFormat.StyleName = styleName;
        paragraph.AppendChild(new AsposeRun(document, text));
        body.AppendChild(paragraph);
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

    private sealed record OutputSetup(
        Handout Handout,
        HandoutVersion HandoutVersion,
        OutputTemplate OutputTemplate,
        OutputForm OutputForm);

    private sealed record ContentBlockSetup(
        ContentBlock ContentBlock,
        ContentBlockVersion Version);

    private sealed record ContentBlockVersionSetup(
        ContentBlock ContentBlock,
        ContentBlockVersion OldVersion,
        ContentBlockVersion CurrentVersion);
}
